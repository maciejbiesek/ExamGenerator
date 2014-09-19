var models = document.getElementById("NumberOfQuestions");
var modelsTable = document.getElementById("itemsmodel");

models.addEventListener("change", drawModels, false);

function drawModels() {
    var modelsNum = parseInt(models.value);
    var curModels = modelsTable.childElementCount;

    if (modelsNum > curModels) {
        var delta = modelsNum - curModels;
        for (var i = 0; i < delta; i++) {
            var input = document.createElement("div");
            input.className = "editor-field";
            input.innerHTML = "@Html.ListBoxFor(x => x.TagIdList, (MultiSelectList)ViewBag.Tags, new { @class = \"chzn-select\", data_placeholder = \"Wybierz tagi...\" })";
            modelsTable.appendChild(input);
        }
    } else {
        while (modelsTable.childElementCount > modelsNum) {
            modelsTable.removeChild(modelsTable.lastChild);
        }
    }
}

drawModels();