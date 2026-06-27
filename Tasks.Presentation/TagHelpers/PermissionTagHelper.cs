using Microsoft.AspNetCore.Razor.TagHelpers;
using Tasks.Domain.Authorization;

namespace Tasks.Presentation.TagHelpers
{
    [HtmlTargetElement("permission", Attributes = "required")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("required")]
        public string Required { get; set; } = string.Empty;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null || !user.HasClaim(Permissions.ClaimType, Required))
            {
                output.SuppressOutput();
                return;
            }

            var content = await output.GetChildContentAsync();
            output.Content.SetHtmlContent(content);
        }
    }
}
