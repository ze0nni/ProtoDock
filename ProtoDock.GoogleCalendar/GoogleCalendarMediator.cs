using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using ProtoDock.Api;

namespace ProtoDock.GoogleCalendar {
	internal sealed class GoogleCalendarMediator : IDockPanelMediator {
		
		private enum State {
			Loading,
			Invalidate,
			Validate
		}
		
		private readonly GoogleCalendarPlugin _plugin;
		private IDockPanelApi _api;
		private State _state;
		private int _lastUpdateMin;

		public CalendarService Service;

		private readonly List<GoogleCalendarIcon> _icons = new List<GoogleCalendarIcon>();

		private PanelScales _scales;

		public GoogleCalendarMediator(GoogleCalendarPlugin plugin) {
			_plugin = plugin;
		}

		public IDockPlugin Plugin => _plugin;
		public void Setup(IDockPanelApi api) {
			_api = api;
		}

		public void UpdateScales(PanelScales scales) {
			_scales = scales;
			foreach (var i in _icons) {
				i.updateGraphics(scales);
			}
		}
		
		public void Awake() {
			ThreadPool.QueueUserWorkItem(_ => Login());
		}
		
		public void Destroy() {
			foreach (var i in _icons) {
				_api.Remove(i, false);
				i.Dispose();
			}
			_icons.Clear();
		}

		public void Update() {
			switch (_state) {
				case State.Loading:
					if (Service != null) {
						_state = State.Invalidate;
						ThreadPool.QueueUserWorkItem(_ => Invalidate());
					}
					break;
				
				case State.Invalidate:
					break;
				
				case State.Validate:
					if (_lastUpdateMin != DateTime.Now.Minute) {
						_state = State.Invalidate;
						ThreadPool.QueueUserWorkItem(_ => Invalidate());
					}
					break;
			}
		}
		
		public void RestoreIcon(int version, string data) {
			
		}

		public bool DragCanAccept(IDataObject data) {
			return false;
		}

		public void DragAccept(int index, IDataObject data) {
			
		}

		private void UpdateEvents(IList<Google.Apis.Calendar.v3.Data.Event> events) {
			_lastUpdateMin = DateTime.Now.Minute;
			
			events = events
				.Where(e => e.Start.DateTime != null)
				.Where(e=> e.Start.DateTime - DateTime.Now < TimeSpan.FromMinutes(15))
				.ToList();
			
			while (_icons.Count < events.Count) {
				var i = new GoogleCalendarIcon(this, _api, _scales);
				_icons.Add(i);
				_api.Add(i, true);
			}
			while (_icons.Count > events.Count) {
				var i = _icons[_icons.Count - 1];
				_icons.Remove(i);
				_api.Remove(i, true);
				i.Dispose();
			}

			for (var i = 0; i < _icons.Count; i++) {
				_icons[i].SetData(events[i]);
			}
		}
		
		private void Login() {
			UserCredential credential;
			string[] Scopes = { CalendarService.Scope.CalendarReadonly };

			using (var stream =
				new FileStream("C:\\Projects\\ProtoDock\\ProtoDock\\Embeded\\credentials.json", FileMode.Open, FileAccess.Read))
			{
				string credPath = "calendar-token.json";
				credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					Scopes,
					"user",
					CancellationToken.None,
					new FileDataStore(credPath, true)).Result;
			}
			var service = new CalendarService(new BaseClientService.Initializer() {
				HttpClientInitializer = credential,
				ApplicationName = "ProtoDock Calendar"
			});

			_api.Dock.InvokeAction(() => {
				Service = service;
			});
		}

		private void Invalidate() {
			try {
				EventsResource.ListRequest request = Service.Events.List("primary");
				request.TimeMin = DateTime.Now;
				request.ShowDeleted = false;
				request.SingleEvents = true;
				request.MaxResults = 3;
				request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
				
				Events events = request.Execute();

				if (events.Items == null) {
					return;
				}
				_api.Dock.InvokeAction(() => {
					_state = State.Validate;
					UpdateEvents(events.Items);
				});
			}
			catch (Exception exc) {
				Debug.Write(exc);
			}
		}
	}
}