namespace Snow.Models
{
  public class Page : Post
  {
    public override void SetSnowSettings(SnowSettings defaults)
    {
      base.SetSnowSettings(defaults);
      Layout = defaults.DefaultPageLayout;
    }
  }
}
