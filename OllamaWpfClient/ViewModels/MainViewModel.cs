using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OllamaWpfClient.Common;
using OllamaWpfClient.Models;
using OllamaWpfClient.Services;

namespace OllamaWpfClient.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IOllamaClient _ollamaClient;

        private OllamaModelInfo? _selectedModel;
        private string _inputText = string.Empty;
        private string _statusMessage = "준비";
        private bool _isBusy;

        private CancellationTokenSource? _cts;

        public MainViewModel(IOllamaClient ollamaClient)
        {
            _ollamaClient = ollamaClient;

            AvailableModels = new ObservableCollection<OllamaModelInfo>();
            Messages = new ObservableCollection<ChatMessage>();

            LoadModelsCommand = new AsyncRelayCommand(LoadModelsAsync, CanLoadModels);
            SendCommand = new AsyncRelayCommand(SendAsync, CanSend);
            CancelCommand = new RelayCommand(Cancel, CanCancel);
            ClearConversationCommand = new RelayCommand(ClearConversation, CanClearConversation);
        }

        public ObservableCollection<OllamaModelInfo> AvailableModels { get; }

        public ObservableCollection<ChatMessage> Messages { get; }

        public OllamaModelInfo? SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel == value)
                {
                    return;
                }
                _selectedModel = value;
                OnPropertyChanged();
                SendCommand.RaiseCanExecuteChanged();
            }
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText == value)
                {
                    return;
                }
                _inputText = value;
                OnPropertyChanged();
                SendCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage == value)
                {
                    return;
                }
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                {
                    return;
                }
                _isBusy = value;
                OnPropertyChanged();
                LoadModelsCommand.RaiseCanExecuteChanged();
                SendCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                ClearConversationCommand.RaiseCanExecuteChanged();
            }
        }

        public AsyncRelayCommand LoadModelsCommand { get; }
        public AsyncRelayCommand SendCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand ClearConversationCommand { get; }

        private bool CanLoadModels()
        {
            return !IsBusy;
        }

        private bool CanSend()
        {
            return !IsBusy
                && SelectedModel != null
                && !string.IsNullOrWhiteSpace(InputText);
        }

        private bool CanCancel()
        {
            return IsBusy;
        }

        private bool CanClearConversation()
        {
            return !IsBusy;
        }

        private async Task LoadModelsAsync()
        {
            IsBusy = true;
            StatusMessage = "모델 목록 불러오는 중...";

            try
            {
                var models = await _ollamaClient.ListModelsAsync();

                AvailableModels.Clear();
                foreach (var model in models)
                {
                    AvailableModels.Add(model);
                }

                if (AvailableModels.Count > 0)
                {
                    SelectedModel = AvailableModels[0];
                    StatusMessage = $"{AvailableModels.Count}개 모델 로드됨";
                }
                else
                {
                    StatusMessage = "설치된 모델이 없습니다. `ollama pull <model>` 실행 필요";
                }
            }
            catch (HttpRequestException ex)
            {
                StatusMessage = $"[네트워크/API 오류] {ex.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"[오류] {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SendAsync()
        {
            if (SelectedModel == null)
            {
                StatusMessage = "모델을 먼저 선택하세요";
                return;
            }

            string userText = InputText.Trim();
            if (string.IsNullOrEmpty(userText))
            {
                return;
            }

            Messages.Add(new ChatMessage("user", userText));
            InputText = string.Empty;

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            IsBusy = true;
            StatusMessage = $"{SelectedModel.Name} 응답 생성 중...";

            try
            {
                ChatMessage reply = await _ollamaClient.ChatAsync(SelectedModel.Name, Messages, token);
                Messages.Add(reply);
                StatusMessage = "완료";
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                Messages.Add(new ChatMessage("error", "[취소됨] 사용자가 요청을 취소했습니다."));
                StatusMessage = "취소됨";
            }
            catch (TaskCanceledException)
            {
                Messages.Add(new ChatMessage("error", "[오류] 요청이 시간 초과되었습니다."));
                StatusMessage = "타임아웃";
            }
            catch (HttpRequestException ex)
            {
                Messages.Add(new ChatMessage("error", $"[네트워크/API 오류] {ex.Message}"));
                StatusMessage = "API 오류";
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage("error", $"[오류] {ex.Message}"));
                StatusMessage = "오류";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Cancel()
        {
            _cts?.Cancel();
            StatusMessage = "취소 요청됨...";
        }

        private void ClearConversation()
        {
            Messages.Clear();
            StatusMessage = "대화 초기화됨";
        }
    }
}
