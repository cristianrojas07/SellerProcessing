using Application.DTOs;
using BlazorApp.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Web;

namespace BlazorApp.Components.Pages;

public partial class Sellers
{
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [Inject] public HttpClient Http { get; set; } = default!;
    [Inject] public IDialogService DialogService { get; set; } = default!;

    private MudTable<SellerDto>? table;
    private bool _isUploading = false;
    private string searchString = "";
    private string regionFilter = "";
    private string firstNameFilter = "";
    private string lastNameFilter = "";
    private string emailFilter = "";

    private async Task<TableData<SellerDto>> ServerReload(TableState state, CancellationToken token)
    {
        try
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            query["page"] = (state.Page + 1).ToString();
            query["pageSize"] = state.PageSize.ToString();

            if (!string.IsNullOrWhiteSpace(searchString)) query["search"] = searchString;
            if (!string.IsNullOrWhiteSpace(regionFilter)) query["region"] = regionFilter;
            if (!string.IsNullOrWhiteSpace(firstNameFilter)) query["firstName"] = firstNameFilter;
            if (!string.IsNullOrWhiteSpace(lastNameFilter)) query["lastName"] = lastNameFilter;
            if (!string.IsNullOrWhiteSpace(emailFilter)) query["email"] = emailFilter;

            var response = await Http.GetFromJsonAsync<PagedList<SellerDto>>($"api/sellers?{query}", token);

            return new TableData<SellerDto>()
            {
                TotalItems = response?.TotalCount ?? 0,
                Items = response?.Items ?? new List<SellerDto>()
            };
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
            return new TableData<SellerDto>() { TotalItems = 0, Items = new List<SellerDto>() };
        }
    }

    private async Task OnFilterChanged(string filterType, string value)
    {
        switch (filterType)
        {
            case "search": searchString = value; break;
            case "region": regionFilter = value; break;
            case "firstName": firstNameFilter = value; break;
            case "lastName": lastNameFilter = value; break;
            case "email": emailFilter = value; break;
        }

        if (table is not null)
        {
            await table.ReloadServerData();
        }
    }

    private async Task OpenCreateDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        var dialog = await DialogService.ShowAsync<EditSellerDialog>("Create New Seller", options);
        var result = await dialog.Result;

        if (!result!.Canceled && result.Data is SellerFormModel model)
        {
            var command = new
            {
                model.FirstName,
                model.LastName,
                model.Email,
                model.PhoneNumber,
                model.Region
            };

            var response = await Http.PostAsJsonAsync("api/sellers", command);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Seller created successfully!", Severity.Success);
                if (table is not null)
                {
                    await table.ReloadServerData();
                }
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error creating seller: {errorMsg}", Severity.Error);
            }
        }
    }

    private async Task OpenEditDialog(SellerDto sellerDto)
    {
        var parameters = new DialogParameters { ["SellerDto"] = sellerDto };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        var dialog = await DialogService.ShowAsync<EditSellerDialog>("Edit Seller", parameters, options);
        var result = await dialog.Result;

        if (!result!.Canceled && result.Data is SellerFormModel model)
        {
            var command = new
            {
                model.FirstName,
                model.LastName,
                model.Email,
                model.PhoneNumber,
                model.Region,
                IsActive = true
            };

            var response = await Http.PutAsJsonAsync($"api/sellers/{sellerDto.Id}", command);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Seller updated!", Severity.Success);
                if (table is not null)
                {
                    await table.ReloadServerData();
                }
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error: {errorMsg}", Severity.Error);
            }
        }
    }

    private async Task DeleteSeller(SellerDto seller)
    {
        var confirm = await DialogService.ShowMessageBox(
            "Delete Confirmation",
            $"Are you sure you want to delete {seller.FirstName} {seller.LastName}?",
            yesText: "Delete", cancelText: "Cancel");

        if (confirm == true)
        {
            var response = await Http.DeleteAsync($"api/sellers/{seller.Id}");
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Seller deleted!", Severity.Success);
                if (table is not null)
                {
                    await table.ReloadServerData();
                }
            }
            else
            {
                Snackbar.Add("Failed to delete seller.", Severity.Error);
            }
        }
    }

    private async Task UploadFiles(IBrowserFile file)
    {
        if (file == null) return;

        _isUploading = true;

        StateHasChanged();

        try
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
            content.Add(fileContent, "file", file.Name);

            var response = await Http.PostAsync("api/sellers/upload", content);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("File uploaded successfully. Refreshing list...", Severity.Success);

                if (table is not null)
                {
                    await table.ReloadServerData();
                }
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Upload failed: {errorMsg}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Upload Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isUploading = false;
        }
    }
}