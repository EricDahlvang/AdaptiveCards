﻿using AdaptiveCards.XamlCardRenderer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlCardVisualizer.Helpers;
using XamlCardVisualizer.ResourceResolvers;

namespace XamlCardVisualizer.ViewModel
{
    public class DocumentViewModel : GenericDocumentViewModel
    {
        private static XamlCardRenderer _renderer;

        private DocumentViewModel(MainPageViewModel mainPageViewModel) : base(mainPageViewModel) { }

        private UIElement _renderedCard;
        public UIElement RenderedCard
        {
            get { return _renderedCard; }
            private set { SetProperty(ref _renderedCard, value); }
        }

        public static async Task<DocumentViewModel> LoadFromFileAsync(MainPageViewModel mainPageViewModel, IStorageFile file, string token)
        {
            var answer = new DocumentViewModel(mainPageViewModel);
            await answer.LoadFromFileAsync(file, token, assignPayloadWithoutLoading: true);
            return answer;
        }

        public static DocumentViewModel LoadFromPayload(MainPageViewModel mainPageViewModel, string payload)
        {
            return new DocumentViewModel(mainPageViewModel)
            {
                _payload = payload
            };
        }

        protected override async void LoadPayload(string payload)
        {
            var newErrors = await PayloadValidator.ValidateAsync(payload);
            if (newErrors.Any(i => i.Type == ErrorViewModelType.Error))
            {
                MakeErrorsLike(newErrors);
                return;
            }

            try
            {
                if (_renderer == null)
                {
                    InitializeRenderer(MainPageViewModel.HostConfigEditor.HostConfig);
                }
            }
            catch (Exception ex)
            {
                newErrors.Add(new ErrorViewModel()
                {
                    Message = "Initializing renderer error: " + ex.ToString(),
                    Type = ErrorViewModelType.Error
                });
                MakeErrorsLike(newErrors);
                return;
            }

            try
            {
                if (Settings.UseAsyncRenderMethod)
                {
                    RenderedCard = await _renderer.RenderAdaptiveJsonAsXamlAsync(payload);
                }
                else
                {
                    RenderedCard = _renderer.RenderAdaptiveJsonAsXaml(payload);
                }

                if (RenderedCard is FrameworkElement)
                {
                    (RenderedCard as FrameworkElement).VerticalAlignment = VerticalAlignment.Top;
                }
                MakeErrorsLike(newErrors);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                newErrors.Add(new ErrorViewModel()
                {
                    Message = "Rendering failed",
                    Type = ErrorViewModelType.Error
                });
                MakeErrorsLike(newErrors);
            }
        }

        public static void InitializeRenderer(AdaptiveHostConfig hostConfig)
        {
            try
            {
                _renderer = new XamlCardRenderer();
                if (hostConfig != null)
                {
                    _renderer.SetHostConfig(hostConfig);
                }

                // Custom resource resolvers
                _renderer.ResourceResolvers.Set("symbol", new MySymbolResourceResolver());

                _renderer.Action += async (sender, e) =>
                {
                    var m_actionDialog = new ContentDialog();

                    if (e.Action.ActionType == ActionType.ShowCard)
                    {
                        AdaptiveShowCardAction showCardAction = (AdaptiveShowCardAction)e.Action;
                        m_actionDialog.Content = await _renderer.RenderCardAsXamlAsync(showCardAction.Card);
                    }
                    else
                    {
                        m_actionDialog.Content = "We got an action!\n" + e.Action.ActionType + "\n" + e.Inputs;
                    }

                    m_actionDialog.PrimaryButtonText = "Close";

                    await m_actionDialog.ShowAsync();
                };
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                throw;
            }
        }
    }
}
