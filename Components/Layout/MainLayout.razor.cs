using Microsoft.AspNetCore.Components;
using ShopOwnerSimulator.State;

namespace ShopOwnerSimulator.Components.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        // Parameterless ctor (Blazor requires default ctor for component activation)
        public MainLayout()
        {
        }

        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        [Inject]
        public GameState GameState { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (GameState != null && GameState.Player == null)
                {
                    await GameState.LoadPlayerAsync();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"MainLayout: failed to initialize GameState: {ex}");
            }
        }
    }
}
