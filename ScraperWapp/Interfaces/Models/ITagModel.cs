namespace ScraperWapp.Data.DTOS;

public interface ITagModel
{
    string Action { get; set; }
    string Method { get; set; }
    List<MetaFormInputModel> Inputs { get; set; }
}
