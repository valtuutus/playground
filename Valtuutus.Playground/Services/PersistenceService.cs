using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Valtuutus.Playground.Models;

namespace Valtuutus.Playground.Services;

public class PersistenceService(IJSRuntime js)
{
    private const string LocalStorageKey = "vtt-playground-state";
    private const string UrlParam = "state";

    public async Task<PlaygroundState?> LoadAsync()
    {
        var urlParam = await js.InvokeAsync<string?>("get_url_param", UrlParam);
        if (!string.IsNullOrEmpty(urlParam))
            return StateSerializer.Decode(urlParam);

        var stored = await js.InvokeAsync<string?>("localStorage_get", LocalStorageKey);
        return StateSerializer.Decode(stored);
    }

    public async Task SaveAsync(PlaygroundState state)
    {
        var encoded = StateSerializer.Encode(state);
        await js.InvokeVoidAsync("localStorage_set", LocalStorageKey, encoded);
    }

    public async Task<string> GetShareUrlAsync(PlaygroundState state)
    {
        var encoded = StateSerializer.Encode(state);
        var currentUrl = await js.InvokeAsync<string>("get_current_url");
        var uri = new Uri(currentUrl);
        var baseUrl = uri.GetLeftPart(UriPartial.Path);
        return $"{baseUrl}?{UrlParam}={encoded}";
    }

    public async Task CopyShareUrlToClipboardAsync(PlaygroundState state)
    {
        var url = await GetShareUrlAsync(state);
        await js.InvokeVoidAsync("set_url", url);
        await js.InvokeVoidAsync("copy_to_clipboard", url);
    }
}
