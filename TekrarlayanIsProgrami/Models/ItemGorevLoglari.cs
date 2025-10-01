namespace TekrarlayanIsProgrami.Models
{
    public class ItemGorevLoglari
    {
        public int ID {  get; set; }
        public int Gorev_ID {  get; set; }
        public int CreUser { get; set; }
        public DateTime CreDate { get; set; }
        public int UpdUser { get; set; }
        public DateTime UpdDate { get; set; }
        public int DelUser { get; set; }
        public DateTime DelDate { get; set; }
    }
}
