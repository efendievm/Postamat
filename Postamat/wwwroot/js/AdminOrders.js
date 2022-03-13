var tokenKey = "accessToken";

/// Получение всех заказов
async function GetOrders() {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/order/all", {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const orders = await response.json();
        let rows = document.getElementById("ordersTable");
        rows.innerHTML = "";
        orders.forEach(order => {
            rows.append(OrderRow(order));
        });
    }
}

/// Получение заказа по id
async function GetOrder(id) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/order/id/" + id, {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const order = await response.json();
        const form = document.forms["orderForm"];
        form.elements["id"].value = order.id;
        form.elements["postamatNumber"].value = order.postamatNumber;
        form.elements["status"].forEach(rb => rb.checked = rb.nextElementSibling.innerText == order.status);
        switch (order.status) {
            case 1: document.getElementById("registered").checked = true;
            case 2: document.getElementById("accepted").checked = true;
            case 3: document.getElementById("issued").checked = true;
            case 4: document.getElementById("deliveredToPost").checked = true;
            case 5: document.getElementById("deliveredToRecipient").checked = true;
        }
    }
}

/// Редактирование заказа
async function EditOrder(id, postamat, status) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("api/order/" + id + "/postamat/" + postamat + "/status/" + status, {
        method: "PUT",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const order = await response.json();
        ResetOrderForm();
        document.querySelector("tr[data-orderRowId='" + order.id + "']").replaceWith(OrderRow(order));
    }
}

/// Удаление заказа
async function DeleteOrder(id) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/order/" + id, {
        method: "DELETE",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const order = await response.json();
        document.querySelector("tr[data-orderRowId='" + order.id + "']").remove();
        ResetProductForm();
    }
}

/// Сброс формы заказа 
function ResetOrderForm() {
    const form = document.forms["orderForm"];
    form.reset();
    form.elements["id"].value = 0;
}

/// Формирование строки с заказом
function OrderRow(order) {

    const tr = document.createElement("tr");
    tr.setAttribute("data-orderRowId", order.id);

    const idTd = document.createElement("td");
    idTd.append(order.id);
    tr.append(idTd);

    const idCustomer = document.createElement("td");
    idCustomer.append(order.customerID);
    tr.append(idCustomer);

    const nameTd = document.createElement("td");
    nameTd.append(order.customerName);
    tr.append(nameTd);

    const phoneTd = document.createElement("td");
    phoneTd.append(order.phoneNumber);
    tr.append(phoneTd);

    const postamatTd = document.createElement("td");
    postamatTd.append(order.postamatNumber);
    tr.append(postamatTd);

    const priceTd = document.createElement("td");
    priceTd.append(order.price);
    tr.append(priceTd);

    const statusTd = document.createElement("td");
    statusTd.append(order.status);
    tr.append(statusTd);

    const linksTd = document.createElement("td");

    const editLink = document.createElement("a");
    editLink.setAttribute("style", "cursor:pointer;padding:15px;");
    editLink.append("Изменить");
    editLink.addEventListener("click", e => {
        e.preventDefault();
        if (order.status != "Отменён")
            GetOrder(order.id);
    });
    linksTd.append(editLink);

    const removeLink = document.createElement("a");
    removeLink.setAttribute("style", "cursor:pointer;padding:15px;");
    removeLink.append("Удалить");
    removeLink.addEventListener("click", e => {
        e.preventDefault();
        DeleteOrder(order.id);
    });
    linksTd.append(removeLink);

    tr.appendChild(linksTd);

    return tr;
}

/// Обработчик кнопки изменения заказа
document.forms["orderForm"].addEventListener("submit", e => {
    e.preventDefault();
    const form = document.forms["orderForm"];
    const id = form.elements["id"].value;
    const postamat = form.elements["postamatNumber"].value;
    let status;
    if (document.getElementById("registered").checked)
        status = 1;
    else if (document.getElementById("accepted").checked)
        status = 2;
    else if (document.getElementById("issued").checked)
        status = 3;
    else if (document.getElementById("deliveredToPost").checked)
        status = 4;
    else
        status = 5;
    EditOrder(id, postamat, status);
});

/// Обработчик кнопки сброса формы заказа
document.getElementById("orderFormReset").addEventListener("click", e => {
    e.preventDefault();
    ResetOrderForm();
})

/// Отображение вкладки заказов и скрытие остальных
document.getElementById("ordersButton").addEventListener("click", e => {
    e.preventDefault();

    var ordersButtonClasses = document.getElementById("ordersButton").classList;
    ordersButtonClasses.remove("btn-outline-secondary")
    if (!ordersButtonClasses.contains("btn-outline-primary")) {
        ordersButtonClasses.add("btn-outline-primary")
    }

    ["postamatsButton", "productsButton"].forEach(buttonID => {
        var buttonClasses = document.getElementById(buttonID).classList;
        buttonClasses.remove("btn-outline-primary")
        if (!buttonClasses.contains("btn-outline-secondary")) {
            buttonClasses.add("btn-outline-secondary")
        }
    })

    document.getElementById("postamatsView").style.display = "none";
    document.getElementById("productsView").style.display = "none";
    document.getElementById("ordersView").style.display = "block";

    GetOrders();
})

/// Выход
document.getElementById("exitButton").addEventListener("click", e => {
    e.preventDefault();
    sessionStorage.removeItem(tokenKey);
    window.location.href = "authorization.html";
});