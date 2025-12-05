using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using TekrarlayanIsProgrami.Models;

namespace TekrarlayanIsProgrami.Services
{
    public class PermissionService
    {
        private readonly DataService _dataService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly SemaphoreSlim _cacheLock = new(1, 1);
        private static readonly IReadOnlyList<PermissionLevelOption> _permissionOptions =
            new ReadOnlyCollection<PermissionLevelOption>(
                new List<PermissionLevelOption>
                {
                    new()
                    {
                        Value = 0,
                        Label = "Gizli (0)",
                        Description = "Sayfa ve menü tamamen gizlenir."
                    },
                    new()
                    {
                        Value = 1,
                        Label = "Okuma (1)",
                        Description = "Sayfa görüntülenir, yazma işlemleri kapalıdır."
                    },
                    new()
                    {
                        Value = 2,
                        Label = "Yazma (2)",
                        Description = "Tam erişim, ekleme/düzenleme yapılabilir."
                    }
                });

        private IReadOnlyList<ItemForm> _cachedForms = Array.Empty<ItemForm>();
        private IReadOnlyDictionary<int, int> _cachedFormPermissions = new Dictionary<int, int>();
        private int? _cachedUserId;

        public PermissionService(DataService dataService, AuthenticationStateProvider authenticationStateProvider)
        {
            _dataService = dataService;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<IReadOnlyList<ItemForm>> GetMenuAsync(bool forceRefresh = false)
        {
            var forms = await GetFormsForCurrentUserAsync(forceRefresh);
            var isAdmin = await IsAdminAsync();

            return forms
                .Where(form => form.IsMenu && (isAdmin || GetCachedPermission(form.ID) > 0))
                .OrderBy(form => form.Sira)
                .ThenBy(form => form.Adi)
                .ToList();
        }

        public IReadOnlyList<PermissionLevelOption> PermissionOptions => _permissionOptions;

        public string GetPermissionDescription(int yetki)
            => _permissionOptions.FirstOrDefault(x => x.Value == yetki)?.Description ?? string.Empty;

        public async Task<bool> CanReadAsync(string? routeOrName)
            => await HasPermissionAsync(routeOrName, minimumYetki: 1);

        public async Task<bool> CanWriteAsync(string? routeOrName)
            => await HasPermissionAsync(routeOrName, minimumYetki: 2);

        public async Task<bool> HasPermissionAsync(string? routeOrName, int minimumYetki)
        {
            if (minimumYetki <= 0)
            {
                return true;
            }

            if (await IsAdminAsync())
            {
                return true;
            }

            var form = await FindFormAsync(routeOrName);
            if (form is null)
            {
                return false;
            }

            return GetCachedPermission(form.ID) >= minimumYetki;
        }

        public async Task RefreshAsync()
        {
            await _cacheLock.WaitAsync();
            try
            {
                _cachedForms = Array.Empty<ItemForm>();
                _cachedFormPermissions = new Dictionary<int, int>();
                _cachedUserId = null;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private async Task<ItemForm?> FindFormAsync(string? routeOrName)
        {
            if (string.IsNullOrWhiteSpace(routeOrName))
            {
                return null;
            }

            var normalizedTarget = NormalizeKey(routeOrName);
            var forms = await GetFormsForCurrentUserAsync();

            return forms.FirstOrDefault(form =>
            {
                var normalizedRoute = NormalizeKey(ResolveRoute(form));
                var normalizedName = NormalizeKey(form.Adi);
                return normalizedRoute == normalizedTarget || normalizedName == normalizedTarget;
            });
        }

        private async Task<IReadOnlyList<ItemForm>> GetFormsForCurrentUserAsync(bool forceRefresh = false)
        {
            var userId = await GetCurrentUserIdAsync();

            if (!userId.HasValue)
            {
                return Array.Empty<ItemForm>();
            }

            if (!forceRefresh && _cachedUserId == userId && _cachedForms.Count > 0)
            {
                return _cachedForms;
            }

            await _cacheLock.WaitAsync();
            try
            {
                if (!forceRefresh && _cachedUserId == userId && _cachedForms.Count > 0)
                {
                    return _cachedForms;
                }

                var formsTask = _dataService.GetFormsAsync();
                var detaylarTask = _dataService.GetKullaniciFormYetkileriAsync(userId.Value);

                await Task.WhenAll(formsTask, detaylarTask);

                var forms = formsTask.Result;
                var detaylar = detaylarTask.Result;

                var yetkiMap = forms.ToDictionary(f => f.ID, _ => 0);
                foreach (var detay in detaylar)
                {
                    yetkiMap[detay.Form_ID] = detay.Yetki;
                }

                _cachedForms = forms;
                _cachedFormPermissions = yetkiMap;
                _cachedUserId = userId;
                return _cachedForms;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private int GetCachedPermission(int formId)
        {
            if (_cachedFormPermissions.TryGetValue(formId, out var yetki))
            {
                return yetki;
            }

            return 0;
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var claim = authState.User?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null && int.TryParse(claim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }

        private static string NormalizeKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Trim().TrimStart('/').ToLowerInvariant();
        }

        private static string ResolveRoute(ItemForm form)
        {
            var routeCandidate = form.SayfaURL;

            if (string.IsNullOrWhiteSpace(routeCandidate))
            {
                routeCandidate = form.Adi;
            }

            if (string.IsNullOrWhiteSpace(routeCandidate))
            {
                routeCandidate = form.ID.ToString();
            }

            routeCandidate = routeCandidate.Trim();
            if (!routeCandidate.StartsWith("/", StringComparison.Ordinal))
            {
                routeCandidate = "/" + routeCandidate.TrimStart('/');
            }

            return routeCandidate;
        }

        private async Task<bool> IsAdminAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user?.Identity?.IsAuthenticated == true && user.IsInRole("Admin");
        }
    }
}

