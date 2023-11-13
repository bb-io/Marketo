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

- **List all emails**
- **Get email info**
- **Get email content**
- **Delete email**

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

- **List landing pages**
- **Get landing page info**
- **Create landing page**
- **Delete landing page**
- **Approve landing page draft**
- **Discard landing page draft**
- **Unapprove landing page (back to draft)**
- **Get landing page full content**

### Forms

- **Get form**
- **List forms created or updated in date range**
- **Get form as HTML for translation**
- **Create new form from translated HTML**

**Get form as HTML for translation** and **Create new form from translated HTML** are intended to be used together in translation flow: you can retrieve a form as HTML file, put it into TMS, then retrieve a translated HTML file and put it back to Marketo. **Create new form from translated HTML** expects the same HTML structure as the structure of the file retrieved with **Get form as HTML for translation**.

## Missing features

- Email templates
- Programs
- Segments
- Smart lists/campaigns
- Tags and tokens

Let us know if you're interested!

## Feedback

Feedback to our implementation of Marketo is always very welcome. Reach out to us using the established channels or create an issue.

<!-- end docs -->
