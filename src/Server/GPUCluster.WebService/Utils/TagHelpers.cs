using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GPUCluster.WebService.Utils
{
    [HtmlTargetElement("button")]
    public class EnabledButton : TagHelper
    {
        [HtmlAttributeName("asp-is-enabled")]
        public bool IsEnabled { set; get; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsEnabled)
            {
                var d = new TagHelperAttribute("disabled", "");
                output.Attributes.Add(d);
            }
            base.Process(context, output);
        }
    }
    [HtmlTargetElement("button")]
    public class DisabledButton : TagHelper
    {
        [HtmlAttributeName("asp-is-disabled")]
        public bool IsDisabled { set; get; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsDisabled)
            {
                var d = new TagHelperAttribute("disabled", "disabled");
                output.Attributes.Add(d);
            }
            base.Process(context, output);
        }
    }
}