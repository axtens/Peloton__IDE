using Windows.ApplicationModel.Resources;

namespace Peloton_IDE.Presentation
{
    public static class JavaScriptLibrariesHelper
    {
        //static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("JavaScriptLibraries");
        public static string GetJavaScriptLibrariesResource(string name)
        {
#if HAS_UNO
            ResourceLoader resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView("JavaScriptLibraries");
#else  
            var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader("JavaScriptLibraries");
#endif
            // return resourceLoader.GetString(name);
            return resourceLoader.GetString(name);
        }

        public static string GetResource(string name)
        {
#if HAS_UNO
            ResourceLoader resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
#else
            var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
#endif
            // return resourceLoader.GetString(name);
            return resourceLoader.GetString(name);
        }
    }
}
