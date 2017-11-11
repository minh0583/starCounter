# starcounter-upload
Starcounter Polymer element to upload files over WebSocket

## Properties

Name               | Type      | Default                                                                          | Description
-------------------|-----------|----------------------------------------------------------------------------------|--------------
`uploadUrl`        | `String`  |                                                                                  | Upload to url to establish WebSocket connection. Should start with `/` and can not include `?` with query string parameters.
`tasks`            | `Array`   | `[]`                                                                             | Queue of files uploading, filled by `starcounter-upload`. Cannot be supplied from outside.
`file-filter`      | `String`  | `""`                                                                             | Select file dialog filter, for example `.zip` or `.png`.
`paste-file-types` | `Array`   | `["image/png", "image/jpeg", "image/pjpeg", "image/bmp", "image/x-windows-bmp"]` |
`sessionId`        | `String`  |                                                                                  | Starcounter sessions id to pass with file upload.
`multiple`         | `Boolean` | `false`                                                                          | Multiple file selection.
`chunk-length`     | `Number`  | `8192`                                                                           | Size of a chunk transfered at a time in bytes.
`auto-upload`      | `Boolean` | `false`                                                                          | Upload file automatically or manually by clicking accept button.
`active`           | `Boolean` | `false`                                                                          | When set to `true` will capture pasted images and upload as file.

## Methods

Name            | Parameters | Description
----------------|------------|-------------
`selectFile`    | None       | Open select file dialog.
`abortAllTasks` | None       | Abort all queue items.

## Events

Name          | Parameters       | Description
--------------|------------------|-------------
`statechange` | { detail: task } | Occurs when file upload state changes, including start upload, upload progress, end upload, abort, fail to upload.

## Task object

```js
{
	file: file, // File object selected by `<input type="file" />` https://developer.mozilla.org/en/docs/Using_files_from_web_applications
	progress: 0, // File upload progress in %
	error: "", // File upload error text, empty string if no errors,
	sizeString: "" // File size in readable format like 512kb, 1mb, 2.5gb
}
```

## Custom upload URL parameters

```html
<starcounter-upload>
	<input type="hidden" name="CustomerParameter" value="CustomValue" slot="parameters" />
</starcounter-upload>
```
## Notes
- `starcounter-upload` is a [hybrid element](https://www.polymer-project.org/2.0/docs/devguide/hybrid-elements). 

## License

MIT
