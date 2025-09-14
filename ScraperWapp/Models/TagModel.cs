namespace ScraperWapp.Data.DTOS
{


    public class TagModel : ITagModel
    {
        public string Action { get; set; }
        public string Method { get; set; }
        public List<MetaFormInputModel> Inputs { get; set; } = new List<MetaFormInputModel>();
    }
}
