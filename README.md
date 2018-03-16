# Contact-Centre Version C# 0.1

Inbound PSTN to Twilio Client Contact Centre Powered by Taskrouter 

Languages: C#, js

This implements:

-  Single channel (Voice)
- 4 departments
- Agent UI based on TaskRouter SDK for low latency
- Twilio Client WebRTC dashboard
- Conference instruction
- Call instruction
- Conference recording
- Call holding
- Call transfers

## Setup
1. Setup a new TwiML App https://www.twilio.com/console/voice/twiml/apps and point it to the domain where you deployed this app (add `/incoming_call` suffix): `https://YOUR_DOMAIN_HERE/incoming_call`
2. Buy a Twilio number https://www.twilio.com/console/phone-numbers/incoming
3. Configure your number to point towards this TwiML App (Voice: Configure With: TwiML App)
4. Define the following env variables in Web.config:

```
 <appSettings>
      <add key="TWILIO_ACME_ACCOUNT_SID" value="" />
      <add key="TWILIO_ACME_AUTH_TOKEN" value="" />
      <add key="TWILIO_ACME_TWIML_APP_SID" value="" />
      <add key="TWILIO_ACME_WORKSPACE_SID" value="" />
      <add key="TWILIO_ACME_MANAGER_WORKFLOW_SID" value="" />
      <add key="TWILIO_ACME_SUPPORT_WORKFLOW_SID" value="" />
      <add key="TWILIO_ACME_SALES_WORKFLOW_SID" value="" />
      <add key="TWILIO_ACME_BILLING_WORKFLOW_SID" value="" />
      <add key="TWILIO_ACME_CALLERID" value="" />
  </appSettings>

```