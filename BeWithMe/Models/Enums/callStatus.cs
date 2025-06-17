namespace BeWithMe.Models.Enums
{
   
        

        public enum CallStatus
        {
            Initiated,  // Call has been initiated but not yet connected
            Connected,  // Call is active and connected
            Disconnected, // Call has been disconnected abnormally
            Ended,      // Call has been ended normally
            Failed      // Call failed to connect
        }

    
}
