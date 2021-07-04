using System.ComponentModel;
using OfficeOpenXml;

namespace SsPvo.Ui.Common
{
    public class SsAppFromExcel
    {
        [DisplayName("Номер заявления")]
        public string AppNum { get; set; }
        [DisplayName("Фамилия")]
        public string LastName { get; set; }
        [DisplayName("Имя")]
        public string FirstName { get; set; }
        [DisplayName("Отчество")]
        public string MiddleName { get; set; }
        [DisplayName("Снилс")]
        public string Snils { get; set; }
        [DisplayName("Конкурс")]
        public string CgName { get; set; }
        [DisplayName("Уровень")]
        public string Level { get; set; }
        [DisplayName("Форма")]
        public string Form { get; set; }
        [DisplayName("Источник финанс.")]
        public string FinSource { get; set; }
        [DisplayName("Статус")]
        public string EpguStatus { get; set; }
        [DisplayName("Дата регистр.")]
        public string RegDate { get; set; }
        [DisplayName("Дата изменения")]
        public string LastModDate { get; set; }
        [DisplayName("Оригинал док. об образ-нии")]
        public string IsOriginal { get; set; }
        [DisplayName("Согласие на зачисл.")]
        public string IsAgreed { get; set; }
        [DisplayName("Дата согласия")]
        public string IsAgreedDate { get; set; }
        [DisplayName("Отзыв согласия на зачисл.")]
        public string IsRevoked { get; set; }
        [DisplayName("Дата отзыва согласия")]
        public string IsRevokedDate { get; set; }
        [DisplayName("Общежитие")]
        public string NeedHostel { get; set; }
        [DisplayName("Рейтинг")]
        public string Rating { get; set; }
        [DisplayName("ЕПГУ")]
        public string EpguId { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";
    }
}
