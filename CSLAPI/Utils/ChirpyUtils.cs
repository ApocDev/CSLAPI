namespace CSLAPI.Utils
{
	public class ChirpyUtils
	{
		public static void ShowCustomMessage(string sender, string message)
		{
			MessageManager.instance.QueueMessage(new CustomMessage(sender, message));
		}

		public static void ShowCitizenMessage(uint citizen, string message)
		{
			ShowCustomMessage(CitizenManager.instance.GetCitizenName(citizen) ?? "Ominous Voice", message);
		}

		public static void ShowRandomCitizenMessage(string message)
		{
			ShowCitizenMessage(CitizenUtils.GetRandomResidentId(), message);
		}

		public static void ShowLocalizedCitizenMessage(uint citizen, string messageId, string keyId)
		{
			MessageManager.instance.QueueMessage(new CitizenMessage(citizen, messageId, keyId));
		}

		public static void ShowMessage(MessageBase message)
		{
			MessageManager.instance.QueueMessage(message);
		}

		#region Nested type: CustomMessage

		internal class CustomMessage : MessageBase
		{
			private readonly string _sender, _message;

			public CustomMessage(string sender, string message)
			{
				_sender = sender;
				_message = message;
			}

			public override string GetSenderName()
			{
				return _sender;
			}

			public override string GetText()
			{
				return _message;
			}
		}

		#endregion
	}
}