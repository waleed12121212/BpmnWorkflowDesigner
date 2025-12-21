using Microsoft.JSInterop;

namespace BpmnWorkflow.Client.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        
        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetThemeAsync(string theme)
        {
            await _jsRuntime.InvokeVoidAsync("eval", $@"
                var link = document.getElementById('theme-style');
                link.href = '_content/Radzen.Blazor/css/{(theme == "dark" ? "material-dark" : "material-base")}.css';
                if ('{theme}' === 'dark') {{
                    document.documentElement.classList.add('dark');
                }} else {{
                    document.documentElement.classList.remove('dark');
                }}
                localStorage.setItem('theme', '{theme}');
            ");
        }

        public async Task<string> GetCurrentThemeAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("eval", "localStorage.getItem('theme') || 'light'");
        }
        
        public async Task ToggleThemeAsync()
        {
            var current = await GetCurrentThemeAsync();
            var next = current == "dark" ? "light" : "dark";
            await SetThemeAsync(next);
        }
        
        public async Task<bool> IsDarkModeAsync()
        {
            return (await GetCurrentThemeAsync()) == "dark";
        }
    }
}
