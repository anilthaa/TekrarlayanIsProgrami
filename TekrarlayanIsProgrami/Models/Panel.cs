namespace TekrarlayanIsProgrami.Models
{
    public class Panel
    {
        public string Baslik { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Renk { get; set; } = string.Empty;
        public Func<int?, int> DegerFunc { get; set; } = userId => 0; // userId = currentUserId
        public Action<int?> OnClick { get; set; } = userId => { };
    }
}
