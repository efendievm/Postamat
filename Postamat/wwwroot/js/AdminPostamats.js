var tokenKey = "accessToken";

/// Получение всех постаматов
async function GetPostamats() {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/postamat/all", {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const postamats = await response.json();
        let rows = document.getElementById("postamatsTable");
        rows.innerHTML = "";
        postamats.forEach(postamat => {
            rows.append(PostamatRow(postamat));
        });
    }
}

/// Получение постамата по id
async function GetPostamat(id) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/postamat/" + id, {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const postamat = await response.json();
        const form = document.forms["postamatForm"];
        form.elements["id"].value = postamat.id;
        form.elements["number"].value = postamat.number;
        form.elements["address"].value = postamat.address;
        if (postamat.isWorking)
            document.getElementById("yes").checked = true;
        else
            document.getElementById("no").checked = true;
    }
}

/// Создание постамата
async function CreatePostamat(postamatNumber, postamatAddress, postamatIsWorking) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("api/postamat", {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token
        },
        body: JSON.stringify({
            number: postamatNumber,
            address: postamatAddress,
            isWorking: postamatIsWorking
        })
    });
    const postamat = await response.json();
    if (response.ok === true) {
        ResetPostamatForm();
        document.getElementById("postamatsTable").append(PostamatRow(postamat));
    }
    else {
        alert("Error: " + response.status + "; " + postamat.errorText);
    }
}

/// Редактирование постамата
async function EditPostamat(postamatId, postamatNumber, postamatAddress, postamatIsWorking) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("api/postamat", {
        method: "PUT",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token
        },
        body: JSON.stringify({
            id: parseInt(postamatId, 10),
            number: postamatNumber,
            address: postamatAddress,
            isWorking: postamatIsWorking
        })
    });
    const postamat = await response.json();
    if (response.ok === true) {
        ResetPostamatForm();
        document.querySelector("tr[data-postamatRowId='" + postamat.id + "']").replaceWith(PostamatRow(postamat));
    }
    else {
        alert("Error: " + response.status + "; " + postamat.errorText);
    }
}

/// Удаление постамата
async function DeletePostamat(id) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/admin/postamat/" + id, {
        method: "DELETE",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const postamat = await response.json();
        document.querySelector("tr[data-postamatRowId='" + postamat.id + "']").remove();
        ResetPostamatForm();
    }
}

/// Сброс формы постамата
function ResetPostamatForm() {
    const form = document.forms["postamatForm"];
    form.reset();
    form.elements["id"].value = 0;
}

/// Формирование строки с постаматом
function PostamatRow(postamat) {

    const tr = document.createElement("tr");
    tr.setAttribute("data-postamatRowId", postamat.id);

    const idTd = document.createElement("td");
    idTd.append(postamat.id);
    tr.append(idTd);

    const numberTd = document.createElement("td");
    numberTd.append(postamat.number);
    tr.append(numberTd);

    const addressTd = document.createElement("td");
    addressTd.append(postamat.address);
    tr.append(addressTd);

    const isWorkingTd = document.createElement("td");
    isWorkingTd.append(postamat.isWorking ? "Открыт" : "Закрыт");
    tr.append(isWorkingTd);

    const linksTd = document.createElement("td");

    const editLink = document.createElement("a");
    editLink.setAttribute("style", "cursor:pointer;padding:15px;");
    editLink.append("Изменить");
    editLink.addEventListener("click", e => {

        e.preventDefault();
        GetPostamat(postamat.id);
    });
    linksTd.append(editLink);

    const removeLink = document.createElement("a");
    removeLink.setAttribute("style", "cursor:pointer;padding:15px;");
    removeLink.append("Удалить");
    removeLink.addEventListener("click", e => {

        e.preventDefault();
        DeletePostamat(postamat.id);
    });

    linksTd.append(removeLink);
    tr.appendChild(linksTd);

    return tr;
}

/// Обработчик кнопки отправки формы постамата
document.forms["postamatForm"].addEventListener("submit", e => {
    e.preventDefault();
    const form = document.forms["postamatForm"];
    const id = form.elements["id"].value;
    const number = form.elements["number"].value;
    const address = form.elements["address"].value;
    const isWorking = document.getElementById("yes").checked;
    if (id == 0)
        CreatePostamat(number, address, isWorking);
    else
        EditPostamat(id, number, address, isWorking);
});

/// Обработчик кнопки сброса формы постамата
document.getElementById("postamatFormReset").addEventListener("click", e => {
    e.preventDefault();
    ResetPostamatForm();
})

/// Отображение вкладки постамата и скрытие остальных
document.getElementById("postamatsButton").addEventListener("click", e => {
    e.preventDefault();

    var postamatsButtonClasses = document.getElementById("postamatsButton").classList;
    postamatsButtonClasses.remove("btn-outline-secondary")
    if (!postamatsButtonClasses.contains("btn-outline-primary")) {
        postamatsButtonClasses.add("btn-outline-primary")
    }
    
    ["productsButton", "ordersButton"].forEach(buttonID => {
        var buttonClasses = document.getElementById(buttonID).classList;
        buttonClasses.remove("btn-outline-primary")
        if (!buttonClasses.contains("btn-outline-secondary")) {
            buttonClasses.add("btn-outline-secondary")
        }
    })
    
    document.getElementById("postamatsView").style.display = "block";
    document.getElementById("productsView").style.display = "none";
    document.getElementById("ordersView").style.display = "none";

    GetPostamats();
})