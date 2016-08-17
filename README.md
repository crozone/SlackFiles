# SlackFiles
A bulk file downloader and deleter for Slack


## About

SlackFiles uses the Slack API to access and delete files within a slack team.
To do this, it requires a Slack auth token, which can be aquired from here: https://api.slack.com/docs/oauth-test-tokens

This application does not currently implement the full OAuth2 workflow required for authenticating with Slack,
which is why testing tokens need to be aquired to use it.
