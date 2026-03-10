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

1. Navigate to apps and search for Marketo.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My client'.
4. Fill in the "Munchkin Account ID", "Client ID" and "Client secret" of your Marketo instance you want to connect to. You can find it in Marketo settings
5. Click _Authorize connection_.

## Actions

### Content

> Note: These actions support 4 different content types: forms, emails, snippets and landing pages

- **Search content** Search for content across multiple content types using specific criteria.
- **Download content** Download content of a specific content type.
- **Upload content** Upload content of a specific content type.

### Emails

- **Search emails** Search emails using specific criteria.
- **Get email** Get information about a specific email.
- **Update email metadata** Update metadata of a specific email.
- **Get email content** Get content of a specific email.
- **Delete email** Delete a specific email.
- **Download email content** Download content of a specific email.
- **Upload email content** Upload content of a specific email.
- **Get email dynamic content** Get dynamic content of a specific email.
- **Get email dynamic image content** Get dynamic image content of a specific email.

### Email templates

- **Search email templates** Search email templates using specific criteria.
- **Get email template** Get information about a specific email template.
- **Get email template content** Get content of a specific email template.
- **Create email template** Create new email template.
- **Update email template content** Update content of a specific email template.
- **Delete email template** Delete a specific email template.
- **Approve email template draft** Approve a specific email template draft.

### Files

- **Search files** Search files.
- **Get file** Get information of a specific file.
- **Download file** Download a specific file.
- **Update file** Update content of a specific file.
- **Upload file** Upload a file.

### Folders

- **Search folders** Search folders using specific criteria.
- **Get folder** Get information of a specific folder.
- **Get folder by name** Get information of a specific folder by its name.
- **Create folder** Create a folder.
- **Delete folder** Delete a specific folder.
- **Add tag to program** Add tag to a specific program.
- **Get program tag** Get value of a specific program tag.

### Landing pages

- **Search landing pages** Search landing pages using specific criteria.
- **Get landing page** Get information of a specific landing page.
- **Get landing page content** Get content of a specific landing page.
- **Update landing page metadata** Update metadata of a specific landing page.
- **Create landing page** Create a landing page.
- **Delete landing page** Delete a specific landing page.
- **Approve landing page draft** Approve a specific landing page draft.
- **Discard landing page draft** Discard a specific landing page draft.
- **Unapprove landing page** Revert a specific landing page back to draft.
- **Download landing page content** Download content of a specific landing page.
- **Upload landing page content** Upload content of a specific landing page.

### Forms

- **Get form** Get information of a specific form.
- **Search forms** Search forms using specific criteria.
- **Search forms fields** Search fields of a specific form.
- **Update form metadata** Update metadata of a specific form.
- **Download form content** Download content of a specific form.
- **Upload form content** Upload content of a specific form.

### Snippets

- **Search snippets** Search snippets using specific criteria.
- **Get snippet** Get information of a specific snippet.
- **Get snippet content** Get content of a specific snippet.
- **Create snippet** Create a snippet.
- **Update snippet metadata** Update metadata of a specific snippet.
- **Download snippet content** Download content of a specific snippet.
- **Upload snippet content** Upload content of a specific snippet.

### Tokens

- **Search tokens** Search tokens in a specific folder.
- **Get token by name** Get information of a specific token by its name.
- **Create token** Create a token.
- **Delete token** Delete a specific token.

### Translation functionality (dynamic content)

- Translation functionality is based on dynamic content in Marketo. This type of translation is available for emails and landing pages (last two actions specified in email and landing page sections above)
- In your Marketo you must have a segmentation which contains segments named as language codes or language names (as you wish). Both actions must use the same segmentation but different segments (because segments are simply different languages).
- Blackbird will automatically convert all text fields (HTML or rich text) to dynamic content during a translation action. If you want to prevent this you can set "Translate only dynamic content" in translation actions to "true" and then only dynamic content fields will be changed (static text content will remain the same). Don't use this flag if you want to translate whole content.
- Fields which are segmented manually from UI by different segmentation (not the one you specified in actions input) will be ignored.
- If you need more control over dynamic content of individual sections of emails and landing pages - please contact us
  
![image](https://github.com/bb-io/Marketo/assets/127740895/ec77cf4a-9468-41b8-a5ee-3ca902183eeb)

## Events

### Emails

- **On emails created or updated**

### Email templates

- **On email templates created or updated**

### Snippets

- **On snippets created or updated**

### Forms

- **On forms created or updated**

### Landing pages
- **On landing pages created or updated**

### Content

> Note: These events support 3 different content types: emails, snippets and landing pages. Forms are not supported

- **On content approved**

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
