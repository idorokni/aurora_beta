using Aurora.Client.WpfApplication.Core;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using System.Threading;
using System.Collections.Concurrent;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.Infrustructure;
using SharedData.Model;
using Aurora.Client.Communication.Managers;

namespace Aurora.Client.WpfApplication.MVVM.ViewModel
{
    class SearchResultViewModel : ObservableObject
    {
        public string UserQuery { get; set; }
        private static readonly SemaphoreSlim
            _semaphoreSlim = new(1, 1);
        public ObservableCollection<SearchModel> SearchResult { get; set; } = new();
        public RelayCommand ViewUserCommand { get; set; }

        public SearchResultViewModel(string userQuery)
        {
            UserQuery = userQuery;
            _ = LoadSearchResultsAsync(userQuery);

            ViewUserCommand = new RelayCommand(async o =>
            {
                if (o is SearchModel data)
                {
                    try
                    {
                        var result = await SocialMediaManager.Instance.GetUserData(data.UserId);
                        var posts = await SocialMediaManager.Instance.GetAllPostsAsync(data.UserId);
                        MainViewModel.Instance.CurrentView = new UserProfileViewModel(data.UserId, result, posts);
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
            });
        }

        private async Task LoadSearchResultsAsync(string query)
        {
            var results = await SocialMediaManager.Instance.SearchQueryAsync(query);
            if (results != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchResult.Clear();
                    results.ForEach(SearchResult.Add);
                });
            }
        }
    }
}
