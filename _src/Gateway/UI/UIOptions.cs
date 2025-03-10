namespace Gateway.UI;

/// <summary>
/// Options for UI configuration
/// </summary>
public class UIOptions
{
    /// <summary>
    /// Gets or sets whether the UI is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the path where the UI is served
    /// </summary>
    public string Path { get; set; } = "/ui";
    
    /// <summary>
    /// Gets or sets the physical path to the UI files
    /// </summary>
    public string PhysicalPath { get; set; } = "UI/BlazorWasm/wwwroot";
    
    /// <summary>
    /// Gets or sets whether authentication is required to access the UI
    /// </summary>
    public bool RequireAuthentication { get; set; } = false;
}
