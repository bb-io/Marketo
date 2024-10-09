# Blackbird.io Marketo

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Marketo is Software-as-a-Service (SaaS)-based marketing automation software owned by Adobe and built to help organizations automate and measure marketing engagement, tasks and workflows.

## Before setting up

Before you can connect you need to make sure that:

- You have a Marketo account on the instance you want to connect to.
- Your Marketo account has the right permissions. You need to have the account with "API user" role to setup a connection.

## Connecting

1. Navigate to apps and search for Marketo. If you cannot find Marketo then click _Add App_ in the top right corner, select Marketo and add the app to your Blackbird environment.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My client'.
4. Fill in the "Munchkin Account ID", "Client ID" and "Client secret" of your Marketo instance you want to connect to. You can find it in Marketo settings
5. Click _Authorize connection_.

## Actions

### Emails

- **Search emails**
- **Get email info**
- **Get email content**
- **Delete email**
- **Get email as HTML for translation**
  <br/>Useful input parameters:
  * "Include images" - if set to true will include images to your exported HTML (default - false)
  * "Get only dynamic content" - if set to true will include only dynamic content from your email (other types of content will be skipped)
  
- **Translate email from HTML file**

### Files

- **List all files**
- **Get file info**
- **Download file**
- **Upload file**

### Folders

- **List folders**
- **Get folder info**
- **Create folder**
- **Delete folder**

### Landing pages

- **Search landing pages**
- **Get landing page info**
- **Create landing page**
- **Delete landing page**
- **Approve landing page draft**
- **Discard landing page draft**
- **Unapprove landing page (back to draft)**
- **Get landing page full content**
- **Get landing page as HTML for translation**
- **Translate landing page from HTML file**

**Get landing page as HTML for translation** and **Translate landing page from HTML file** are intended to be used together in translation flow: you can retrieve a form as HTML file, put it into TMS, then retrieve a translated HTML file and put it back to Marketo. **Translate landing page from HTML file** expects the same HTML structure as the structure of the file retrieved with **Get landing page as HTML for translation**.


### Forms

- **Get form**
- **Search forms created or updated in date range**
- **Get form as HTML for translation**
- **Create new form from translated HTML**

**Get form as HTML for translation** and **Create new form from translated HTML** are intended to be used together in translation flow: you can retrieve a form as HTML file, put it into TMS, then retrieve a translated HTML file and put it back to Marketo. **Create new form from translated HTML** expects the same HTML structure as the structure of the file retrieved with **Get form as HTML for translation**.

### Translation functionality (dynamic content)
- Translation functionality is based on dynamic content in Marketo. This type of translation is available for emails and landing pages (last two actions specified in email and landing page sections above)
- In your Marketo you must have a segmentation which contains segments named as language codes or language names (as you wish). Both actions must use the same segmentation but different segments (because segments are simply different languages).
- Blackbird will automatically convert all text fields (HTML or rich text) to dynamic content during a translation action. If you want to prevent this you can set "Translate only dynamic content" in translation actions to "true" and then only dynamic content fields will be changed (static text content will remain the same). Don't use this flag if you want to translate whole content.
- Fields which are segmented manually from UI by different segmentation (not the one you specified in actions input) will be ignored.
- If you need more control over dynamic content of individual sections of emails and landing pages - please contact us
  
![image](https://github.com/bb-io/Marketo/assets/127740895/ec77cf4a-9468-41b8-a5ee-3ca902183eeb)

## Events

- **On emails created or updated**
- **On email templates created or updated**
- **On snippets created or updated**
- **On forms created or updated**
- **On landing pages created or updated**

## HTML features

We add metadata to the HTML file to include ids. This metadata is used to update the content from the HTML file. These tags are used to identify the content in the `Marketo`, eliminating the need to store IDs elsewhere.

Example of how we include metadata in the HTML file:

```html
<html>
<head>
    <meta name="blackbird-email-id" content="1001">
</head>
<body>
    <h1>Example title</h1>
    <p>Example content</p>
</body>
</html>
```

## Missing features

- Smart lists/campaigns
- Tags and tokens

Let us know if you're interested!

## Feedback

Feedback to our implementation of Marketo is always very welcome. Reach out to us using the established channels or create an issue.

<!-- end docs -->
