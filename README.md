# AI Chat with Custom Data

This project is an AI chat application that demonstrates how to chat with custom data using an AI language model. Please note that this template is currently in an early development stage.

>[!NOTE]
> Before running this project you need to configure the API keys or endpoints for the providers you have chosen. See below for details specific to your choices.

# Configure the AI Model Provider
To use models hosted by Gemini, OpenAI, or Antropic, you need to configure the API keys or endpoints for the providers you have chosen. See below for details for specific to your choices.

From the command line, configure your token for this project using .NET User Secrets by running the following commands:

## PDF Settings
 By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
 other sources by implementing IIngestionSource.
 Important: ensure that any content you ingest is trusted, as it may be reflected back
 to users or could be a source of prompt injection risk.

```sh
cd <<your-project-directory>>
 Setting up secret keys

- dotnet user-secrets init
- dotnet user-secrets set "OpenAI:ApiKey" "yourapikey"

```

Learn more about [prototyping with AI models using GitHub Models](https://docs.github.com/github-models/prototyping-with-ai-models).

