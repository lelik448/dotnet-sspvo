using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Xml;
using System.Threading;

namespace ConsoleApp2
{
    public class RepsonseSS
    {
        public string json;
        public string xml;
    }

    public class SSClient
    {
        private readonly Crypto _csp;
        private readonly string _apiHost;
        private readonly string _ogrn;
        private readonly string _kpp;

        public SSClient(string ogrn, string kpp, string apiHost, Crypto csp)
        {
            if (string.IsNullOrWhiteSpace(ogrn)) throw new ArgumentNullException(nameof(ogrn));
            if (string.IsNullOrWhiteSpace(kpp)) throw new ArgumentNullException(nameof(kpp));
            if (string.IsNullOrWhiteSpace(apiHost)) throw new ArgumentNullException(nameof(apiHost));

            _csp = csp ?? throw new ArgumentNullException(nameof(csp));
            _ogrn = ogrn;
            _kpp = kpp;
            _apiHost = apiHost;
        }

        public async Task<bool> saveXmlBySnils(string snils, string path, string[] appUids = null)
        {
            Console.WriteLine("Обработка СНИЛС:" + snils);
            uint idJwt = await getSnilsIdJwt(snils);
            //Console.WriteLine("IdJwt:" + idJwt);
            RepsonseSS resp = await getServiceQueueMsg(idJwt);
            /*
            Console.WriteLine(resp.json);
            Console.WriteLine("\n\n");
            Console.WriteLine(resp.xml);
            */
            string fname = path + "\\" + snils;
            writeFile(fname + ".xml", resp.xml);
            writeFile(fname + ".json", resp.json);
            bool snilsConfirmed = await confirmMessage(idJwt);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(resp.xml);
            var tags = new string[] { "Identifications", "Documents", "EpguEntrantAchievement", "AppAchievement", "Contract" };
            //var tags = new string[] { "EpguEntrantAchievement" };

            foreach (string tag in tags)
            {
                
                XmlNodeList nodes = xmlDoc.SelectNodes($"//{tag}//UIDEpgu");
                if (nodes.Count>0)
                {
                    Console.WriteLine($"{tag}\n");
                }

                foreach (XmlNode node in nodes)
                {
                    string uid = node.InnerText;
                    uint tagIdJwt = 0;
                    if (tag == "Identifications")
                    {
                        tagIdJwt = await getIdentificationIdJwt(snils, uid);
                    }
                    else if (tag == "Documents")
                    {
                        tagIdJwt = await getDocumentIdJwt (snils, uid);
                    }
                    // При получении заявления из очереди epgu tag AppAchievement,  при запросе ServiceEntrant - EpguEntrantAchievement
                    else if (tag == "EpguEntrantAchievement" || tag == "AppAchievement") 
                    {
                        if (appUids == null || !appUids.Any()) continue;
                        // для тестов возьмём хотя бы одно заявление
                        var appUid = appUids[0];
                        tagIdJwt = await getAchievementIdJwt(appUid, uid);
                    }
                    else if (tag == "Contract")
                    {
                        tagIdJwt = await getContractIdJwt(snils, uid);
                    }

                    //Console.WriteLine(tag + " idJwt:" + tagIdJwt);

                    if (tagIdJwt > 0)
                    {
                        RepsonseSS tagResp = await getServiceQueueMsg(tagIdJwt);
                        string fnameOther = $"{path}\\{snils}-";
                        if (tag == "Documents")
                        {
                            string docType = node.ParentNode.ParentNode.Name;
                            fnameOther += $"{docType}-{uid}";
                        }
                        else
                        {
                            fnameOther += $"{tag}-{uid}";
                        }
                        writeFile(fnameOther + ".xml", tagResp.xml);
                        writeFile(fnameOther + ".json", tagResp.json);
                        bool confirmed = await confirmMessage(tagIdJwt);
                        var xmlOtherDoc = new XmlDocument();
                        xmlOtherDoc.LoadXml(tagResp.xml);
                        XmlNode nodeFile = xmlOtherDoc.SelectSingleNode("//Base64File");
                        if (nodeFile == null )
                        {
                            // Achievement
                            nodeFile = xmlOtherDoc.SelectSingleNode(".//File//Base64");
                        }
                        if (nodeFile != null)
                        {
                            var fileExt = nodeFile.ParentNode.SelectSingleNode(".//FileType");
                            string extension = fileExt != null ? fileExt.InnerText : ".unknown";
                            if (extension[0]!='.') // в Achievement расширение без точки
                            {
                                extension = "." + extension;
                            }
                            byte[] bytes = Convert.FromBase64String(nodeFile.InnerText);
                            writeFile(fnameOther + extension, bytes);
                        }
                    }
                }
            }

            // Обработка заявлений
            if (appUids != null)
            {
                foreach(string appUid in appUids)
                {
                    uint appIdJwt = await getApplicationIdJwt(appUid);
                    RepsonseSS appResp = await getServiceQueueMsg(appIdJwt);
                    if (appResp!=null)
                    {
                        string appFileName = path + "\\" + snils + "-application-" + appUid;
                        writeFile(appFileName + ".xml", appResp.xml);
                        writeFile(appFileName + ".json", appResp.json);
                    }
                }
            }
            return true;
        }

        public async Task<uint> getApplicationIdJwt(string uidEpgu)
        {
            String header = new JObject
            {
                {"action", "get"},
                {"entityType", "serviceApplication"},
                {"ogrn", _ogrn},
                {"kpp", _kpp}
            }.ToString();
            var payload = $"<PackageData><ServiceApplication><IDApplicationChoice><UIDEpgu>{uidEpgu}</UIDEpgu></IDApplicationChoice></ServiceApplication></PackageData>";
            var response = await sendMessage("/api/token/new", header, payload);
            var json = response.Content;
            uint idJwt = getIdJwtFromReponse(json);
            return idJwt;
        }

        public async Task<uint> getSnilsIdJwt(string snils)
        {
            String header = new JObject
            {
                {"action", "get"},
                {"entityType", "serviceEntrant"},
                {"ogrn", _ogrn},
                {"kpp", _kpp}
            }.ToString();
            var payload = $"<PackageData><ServiceEntrant><IDEntrantChoice><SNILS>{snils}</SNILS></IDEntrantChoice></ServiceEntrant></PackageData>";
            var response = await sendMessage("/api/token/new", header, payload);
            var json = response.Content;
            uint idJwt = getIdJwtFromReponse(json);
            return idJwt;
        }

        public async Task<uint> getDocumentIdJwt(string snils, string documentGuid)
        {
            return await getEntityIdJwt("document", "IDDocChoice", snils, documentGuid);
        }

        public async Task<uint> getIdentificationIdJwt(string snils, string documentGuid)
        {
            return await getEntityIdJwt("identification", "IDChoice", snils, documentGuid);
        }

        public async Task<uint> getAchievementIdJwt(string appUid, string documentGuid)
        {
            /*
             Я знаю, Вы так-таки будете смеяться, но этот пакет отличается
             ApplicationIDChoice вместо IDEntrantChoice  :)
             */
            string header = new JObject
            {
                {"action", "get"},
                {"entityType", "appAchievement"},
                {"ogrn", _ogrn},
                {"kpp", _kpp}
            }.ToString();
            var payload = $"<PackageData><AppAchievement><ApplicationIDChoice><UIDEpgu>{appUid}</UIDEpgu></ApplicationIDChoice><AchievementIDChoice><UIDEpgu>{documentGuid}</UIDEpgu></AchievementIDChoice></AppAchievement></PackageData>";
            var response = await sendMessage("/api/token/new", header, payload);
            var json = response.Content;
            return getIdJwtFromReponse(json);
        }

        public async Task<uint> getContractIdJwt(string snils, string documentGuid)
        {
            return await getEntityIdJwt("contract", "IDChoice", snils, documentGuid);
        }

        public async Task<uint> getEntityIdJwt(string entity, string choiceName, string snils, string documentGuid)
        {
            String header = new JObject
            {
                {"action", "get"},
                {"entityType", entity},
                {"ogrn", _ogrn},
                {"kpp", _kpp}
            }.ToString();
            string xmlEmtityTag = capitalizeFirstLetter(entity);
            var payload = $"<PackageData><{xmlEmtityTag}><IDEntrantChoice><SNILS>{snils}</SNILS></IDEntrantChoice><{choiceName}><UIDEpgu>{documentGuid}</UIDEpgu></{choiceName}></{xmlEmtityTag}></PackageData>";
            var response = await sendMessage("/api/token/new", header, payload);
            var json = response.Content;
            //Console.WriteLine($"entity {entity} Response:" + json);
            return getIdJwtFromReponse(json);
        }

        public async Task<RepsonseSS> getServiceQueueMsg(uint idJwt, int retryCount = 5, int msRetryDelay = 2000)
        {
            var header = new JObject
            {
                {"action", "getMessage"},
                {"idJwt", idJwt},
                {"ogrn", _ogrn},
                {"kpp", _kpp}
            };

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    var response = await sendMessage("/api/token/service/info", header.ToString(), "");
                    var json = response.Content;
                    return parseReponseToken(json);
                }
                catch
                {
                    Console.WriteLine($"No response Token Sleep for {msRetryDelay / 1000} seconds.");
                    await Task.Delay(msRetryDelay);
                }
            }

            return null;
        }

        private uint getIdJwtFromReponse(String response)
        {
            Dictionary<String, String> dict = JsonConvert.DeserializeObject<Dictionary<String, String>>(response);
            if (!dict.ContainsKey("idJwt"))
            {
                Console.WriteLine("No idJwt in reponse!");
            }
            return uint.Parse(dict["idJwt"]); ;
        }

        public async Task<IRestResponse> sendMessage(String url, String header, String payload)
        {

            JObject json = getSignedObject(_csp, header, payload);
            //Console.WriteLine(json);
            var request = new RestRequest(url, Method.POST);
            //            request.AddHeader("Content-type", "application/json");
            request.AddParameter("application/json", json.ToString(), ParameterType.RequestBody);
            //Console.WriteLine(request.Body.ToString());
            var client = new RestClient(_apiHost);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("HTTP StatusCode:" + response.StatusCode);
            }
            Dictionary<String, String> result = JsonConvert.DeserializeObject<Dictionary<String, String>>(response.Content);
            if (result.ContainsKey("error"))
            {
                Console.WriteLine("Error:" + result["error"]);
            }
            return response;
        }

        public async Task<Boolean> confirmMessage(uint idJwt)
        {
            // Один и тот же пакет для confirm обоих очередей (service и epgu)

            string header = new JObject
            {
                {"action", "messageConfirm"},
                {"idJwt", idJwt},
                {"ogrn", _ogrn},
                {"kpp", _kpp}
            }.ToString();
            var response = await sendMessage("/api/token/confirm", header, "");
            string json = response.Content;
            Dictionary<String, String> result = JsonConvert.DeserializeObject<Dictionary<String, String>>(json);
            if (!result.ContainsKey("result"))
            {
                Console.WriteLine("Error no Result on Confirm:" + json);
            }
            else if (result["result"] != "true")
            { // :)
                Console.WriteLine("Bad confirm result:" + json);
                return false;
            }
            return true;
        }


        public static JObject getSignedObject(Crypto cryptoService, String header, String payload)
        {
            //string joHeader = msgJwt.JHeader;
            string b64Header = ToBase64String(header);
            string b64Payload = ToBase64String(payload);
            string stringToSign = $"{b64Header}.{b64Payload}";

            try
            {
                byte[] signed = cryptoService.SignData(System.Text.Encoding.UTF8.GetBytes(stringToSign));
                string signature = Convert.ToBase64String(signed);
                return new JObject
                {
                    { "token", $"{b64Header}.{b64Payload}.{signature}" }
                };
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static RepsonseSS parseReponseToken(String response)
        {
            var result = JsonConvert.DeserializeObject<Dictionary<String, String>>(response);

            if (result.ContainsKey("responseToken"))
            {
                string responseToken = result["responseToken"];
                string[] parts = responseToken.Split('.');
                RepsonseSS resp = new RepsonseSS
                {
                    json = FromBase64String(parts[0]),
                    xml = FromBase64String(parts[1])
                };
                return resp;
            }
            else
            {
                Console.WriteLine("No jsOnToken in Reponse:" + response);
                throw new NullReferenceException("No jsOnToken in Reponse!");
            }
        }

        private static string ToBase64String(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string FromBase64String(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static string capitalizeFirstLetter(string text)
        {
            return text?.First().ToString().ToUpper() + text?.Substring(1);
        }

        private static void writeFile(string fname, string text)
        {
            EnsureDirectoryExists(fname);

            FileStream file = new FileStream(fname, FileMode.Create);
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            file.Write(bytes, 0, bytes.Length);
            file.Close();
        }

        private static void writeFile(string fname, byte[] bytes)
        {
            EnsureDirectoryExists(fname);

            FileStream file = new FileStream(fname, FileMode.Create);
            file.Write(bytes, 0, bytes.Length);
            file.Close();
        }

        private static void EnsureDirectoryExists(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }
    }
}