// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function parseDockerStream(dockerJson, updateCurrent, appendNextLine) {
    if ("stream" in dockerJson) {
        appendNextLine(dockerJson["stream"]);
    }
    else if ("aux" in dockerJson) {
        const aux = dockerJson["aux"]
        for (const property in aux) {
            appendNextLine(`${property}: ${aux[property]}`);
        }
    }
    else if ("error" in dockerJson) {
        appendNextLine(`error: ${dockerJson["error"]}`)
        if ("errorDetail" in dockerJson) {
            appendNextLine(dockerJson["errorDetail"]["message"]);
        }
    }
    else if ("id" in dockerJson) {
        updateCurrent(dockerJson["id"], dockerJson["status"], dockerJson["progressDetail"], dockerJson["progress"]);
    }
    else if ("status" in dockerJson) {
        appendNextLine(dockerJson["status"]);
    }
    else {
        console.log(dockerJson)
        for (const property in dockerJson) {
            appendNextLine(`${property}: ${dockerJson[property]}`);
        }
    }
}