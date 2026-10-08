namespace ProjectManagementSystem.WPF.Helpers
{
    public static class ValidationLimits
    {
        public const int PersonNameMin = 2;
        public const int PersonNameMax = 50;
        public const int LoginMin = 3;
        public const int LoginMax = 100;
        public const int PasswordMin = 6;
        public const int PasswordMax = 100;
        public const int ProjectNameMin = 2;
        public const int ProjectNameMax = 100;
        public const int ProjectDescriptionMax = 500;
        public const int TaskTitleMin = 2;
        public const int TaskTitleMax = 100;
        public const int TaskDescriptionMax = 1000;
        public const int CommentMax = 1000;
        public const int SearchMax = 200;
        public const int MaxAttachmentSizeBytes = 10 * 1024 * 1024;
        public const int MaxAttachmentsPerSelection = 5;
    }
}
