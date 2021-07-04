namespace SsPvo.Client.Enums
{
    public enum SsPvoEntityType
    {
        /// <summary> 
        /// кафедра 
        /// </summary>
        SubdivisionOrg,
        /// <summary> 
        /// приемная кампания 
        /// </summary>
        Campaign,
        /// <summary> 
        /// индивидуальное достижение 
        /// </summary>
        Achievement,
        /// <summary> 
        /// КЦП 
        /// </summary>
        AdmissionVolume,
        /// <summary> 
        /// распределение КЦП 
        /// </summary>
        DistributedAdmissionVolume,
        /// <summary> 
        /// конкурсная группа 
        /// </summary>
        CompetitiveGroup,
        /// <summary> 
        /// образовательная программа 
        /// </summary>
        CompetitiveGroupProgram,
        /// <summary> 
        /// льгота конкурсной группы 
        /// </summary>
        CompetitiveBenefit,
        /// <summary> 
        /// вступительные испытания конкурсной группы 
        /// </summary>
        EntranceTest,
        /// <summary> 
        /// льгота в рамках вступительного испытания 
        /// </summary>
        EntranceTestBenefit,
        /// <summary> 
        /// календарь вступительных испытаний 
        /// </summary>
        EntranceTestLocation,
        /// <summary> 
        /// ведомость вступительных испытаний 
        /// </summary>
        EntranceTestSheet,
        /// <summary> 
        /// абитуриент 
        /// </summary>
        Entrant,
        /// <summary> 
        ///  документ, удостоверяющий личность 
        /// </summary>
        Identification,
        /// <summary> 
        /// документы абитуриента 
        /// </summary>
        Document,
        /// <summary> 
        /// заявление 
        /// </summary>
        ApplicationList,
        /// <summary> 
        /// редактирование статуса заявления 
        /// </summary>
        EditApplicationStatusList,
        /// <summary> 
        /// загрузка согласий на испытание 
        /// </summary>
        EntranceTestAgreedList,
        /// <summary> 
        /// загрузка результатов испытаний 
        /// </summary>
        EntranceTestResultList,
        /// <summary> 
        /// загрузка приказа о зачислении 
        /// </summary>
        OrderAdmissionList,
        /// <summary> 
        /// загрузка конкусрных/ рейтинговых списков 
        /// </summary>
        CompetitiveGroupsApplicationsRating,
        /// <summary> 
        /// достижения к заявлению 
        /// </summary>
        AppAchievements,
        /// <summary> 
        /// отправка дат вступительных испытаний в ЕПГУ 
        /// </summary>
        SentToEpguEtc
    }
}
