namespace ScraperWapp.Data.DTOS
{
    public class TagDto
    {
        public string Action { get; set; }
        public string Method { get; set; }
        public List<MetaFormInputDto> Inputs { get; set; } = new List<MetaFormInputDto>();
    }
}
