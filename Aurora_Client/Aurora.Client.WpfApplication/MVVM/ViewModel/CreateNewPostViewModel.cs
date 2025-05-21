using Aurora.Client.WpfApplication.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Controls;
using Aurora.Client.Communication.Services;
using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Infrustructure;

namespace Aurora.Client.WpfApplication.MVVM.ViewModel
{
    class CreateNewPostViewModel : ObservableObject
    {
        private string _imagePath;
        private string _description;
        private Visibility _imageVisibility = Visibility.Collapsed;
        private Visibility _buttonVisibility = Visibility.Visible;

        public string ImagePath { get { return _imagePath; }  set { _imagePath = value; OnPropertyChanged(); } }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(); } }
        public Visibility ImageVisibility { get { return _imageVisibility; } set { _imageVisibility = value; OnPropertyChanged(); } }
        public Visibility ButtonVisibility { get { return _buttonVisibility; } set { _buttonVisibility = value; OnPropertyChanged(); } }

        public RelayCommand ChooseImageCommand { get; set; }
        public RelayCommand SubmitImageCommand { get; set; }

        public CreateNewPostViewModel()
        {

            ChooseImageCommand = new RelayCommand(o =>
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                bool? success = fileDialog.ShowDialog();
                if (success == true)
                {
                    ImagePath = fileDialog.FileName;
                    ImageVisibility = Visibility.Visible;
                    ButtonVisibility = Visibility.Collapsed;
                }
            });

            SubmitImageCommand = new RelayCommand(async o =>
            {
                var info = await UploadPostToServer(Description, ImagePath);
                if (info.code == ResponseCode.ADD_POST_SUCCESS)
                {
                    MainViewModel.Instance.CurrentView = HomeViewModel.Instance;
                }
            });
        }

        private static async Task<ResponseInfo> UploadPostToServer(string postDescription, string imagePath)
        {
            RequestInfo requestInfo = new RequestInfo
            {
                message = Newtonsoft.Json.JsonConvert.SerializeObject(new { Description = postDescription, ImageData = await ImageService.ConvertImagePathToBit(imagePath) }),
                code = RequestCode.ADD_POST_REQUEST_CODE
            };

            await Communicator.Instance.SendMessageToServer(Communicator.Instance.Client, requestInfo);
            var responseInfo = await Communicator.Instance.ReadMessageFromServer(Communicator.Instance.Client);

            return responseInfo;
        }


    }
}
