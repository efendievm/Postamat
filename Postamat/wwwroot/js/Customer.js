var tokenKey = "accessToken";

// #region PostamatsPage 

/// Получение всех постаматов
async function GetPostamats() {
    const response = await fetch("/api/postamat", {
        method: "GET",
        headers: { "Accept": "application/json" }
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

/// Получение постама по номеру
async function GetPostamat(number) {
    const response = await fetch("/api/postamat/number/" + number, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const postamat = await response.json();
        const form = document.forms["postamatForm"];
        form.elements["number"].value = postamat.number;
        form.elements["address"].value = postamat.address;
    }
}

/// Добавление строки с постаматом в таблицу
function PostamatRow(postamatNumber) {

    const tr = document.createElement("tr");

    const numberTd = document.createElement("td");
    numberTd.append(postamatNumber);
    tr.append(numberTd);

    const linksTd = document.createElement("td");

    const infoLink = document.createElement("a");
    infoLink.setAttribute("style", "cursor:pointer;padding:15px;");
    infoLink.append("Информация");
    infoLink.addEventListener("click", e => {

        e.preventDefault();
        GetPostamat(postamatNumber);
    });
    linksTd.append(infoLink);

    tr.appendChild(linksTd);

    return tr;
}

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

// #endregion

// #region ProductsPage

/// Получение всех товаров
/// productsTable - таблица для отображения списка товаров
/// productRow - функция, формирующая строку с товаром
async function GetProducts(productsTable, productRow) {
    const response = await fetch("/api/product", {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const products = await response.json();
        let rows = document.getElementById(productsTable);
        rows.innerHTML = "";
        products.forEach(product => {
            rows.append(productRow(product));
        });
    }
}

/// Формирование строки с товаром для таблицы во вкладке товаров
function ProductRow(product) {

    const tr = document.createElement("tr");

    const numberTd = document.createElement("td");
    numberTd.append(product.name);
    tr.append(numberTd);

    const priceTd = document.createElement("td");
    priceTd.append(product.price);
    tr.append(priceTd);

    return tr;
}

/// Отображение вкладки товаров и скрытие остальных
document.getElementById("productsButton").addEventListener("click", e => {
    e.preventDefault();

    var productsButtonClasses = document.getElementById("productsButton").classList;
    productsButtonClasses.remove("btn-outline-secondary");
    if (!productsButtonClasses.contains("btn-outline-primary")) {
        productsButtonClasses.add("btn-outline-primary")
    }

    ["postamatsButton", "ordersButton"].forEach(buttonID => {
        var buttonClasses = document.getElementById(buttonID).classList;
        buttonClasses.remove("btn-outline-primary")
        if (!buttonClasses.contains("btn-outline-secondary")) {
            buttonClasses.add("btn-outline-secondary")
        }
    })

    document.getElementById("postamatsView").style.display = "none";
    document.getElementById("productsView").style.display = "block";
    document.getElementById("ordersView").style.display = "none";

    GetProducts("productsTable", ProductRow);
})

// #endregion

// #region OrderPage

/// текущий список товаров в заказе
var customerProducts;

/// Получение всех заказов
async function GetOrders() {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/order", {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        // отображаем список заказов
        document.getElementById("ordersTable").style.display = "block";
        // скрываем форму создания/редактирования заказа
        const form = document.forms["orderForm"];
        form.style.display = "none";
        form.reset();

        const orders = await response.json();
        let rows = document.getElementById("ordersTableBody");
        rows.innerHTML = "";
        orders.forEach(order => {
            rows.append(OrderRow(order));
        });
    }
}

/// Получение заказа по id
async function GetOrder(id) {
    // скрываем список заказов
    document.getElementById("ordersTable").style.display = "none";
    // отображаем форму редактирования заказа
    const form = document.forms["orderForm"];
    form.style.display = "block";
    form.elements["postamatNumber"].readOnly = true;
    form.reset();
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/order/" + id, {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    // очищаем текущий список товаров в заказе
    customerProducts = [];
    if (response.ok === true) {
        const order = await response.json();
        form.elements["id"].value = order.id;
        form.elements["customerName"].value = order.customerName;
        form.elements["phoneNumber"].value = order.phoneNumber;
        form.elements["postamatNumber"].value = order.postamatNumber;
        form.elements["status"].value = order.status;
        form.elements["price"].value = order.price;
        // отправляем запрос на получение списка всех товаров
        await GetProducts("customerProductsTable", CustomerProductRow);
        order.products.forEach(product => {
            // добавление товара в текущий список продуктов в заказе
            customerProducts.push(product);
            // получаем ячейку в таблице всех товаров с количеством текущего продукта в заказе
            const productCountCell = document.querySelector("tr[data-productRowName='" + product + "']").cells[2];
            // увеличиваем количество текущего товара в таблице продуктов
            productCountCell.innerHTML = parseInt(productCountCell.innerHTML) + 1;
        })
    }
}

/// Создание заказа
async function CreateOrder(name, phone, postamatNumber, products) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/order", {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token
        },
        body: JSON.stringify({
            postamatNumber: postamatNumber,
            customerName: name,
            phoneNumber: phone,
            products: products
        })
    });
    if (response.ok === true) {
        const order = await response.json();
        document.getElementById("ordersTableBody").append(OrderRow(order));
        const form = document.forms["orderForm"];
        form.elements["status"].value = order.status;
        form.elements["price"].value = order.price;
    }
    else {
        if (response.status === 403)
            alert("Error: " + response.status + "; Chosen postamat is closed");
        else {
            const data = await response.json();
            alert("Error: " + response.status + "; " + data.errorText);
        }
    }
}

/// Редактирование заказа
async function EditOrder(id, name, phone, products) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/order", {
        method: "PUT",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token
        },
        body: JSON.stringify({
            id: id,
            customerName: name,
            phoneNumber: phone,
            products: products
        })
    });
    if (response.ok === true) {
        const order = await response.json();
        document.querySelector("tr[data-orderRowId='" + order.id + "']").replaceWith(OrderRow(order));
        const form = document.forms["orderForm"];
        form.elements["status"].value = order.status;
        form.elements["price"].value = order.price;
    }
    if (response.status === 403)
        alert("Error: " + response.status + "; Chosen postamat is closed");
    else {
        const data = await response.json();
        alert("Error: " + response.status + "; " + data.errorText);
    }
}

/// Отмена заказа
async function CancelOrder(id) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/order/cancel/" + id, {
        method: "Put",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    customerProducts = [];
    if (response.ok === true) {
        const order = await response.json();
        document.querySelector("tr[data-orderRowId='" + order.id + "']").replaceWith(OrderRow(order));
    }
}

/// Формирование строки с заказом
function OrderRow(order) {

    const tr = document.createElement("tr");
    tr.setAttribute("data-orderRowId", order.id);

    const idTd = document.createElement("td");
    idTd.append(order.id);
    tr.append(idTd);

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

    const infoLink = document.createElement("a");
    infoLink.setAttribute("style", "cursor:pointer;padding:15px;");
    infoLink.append("Изменить");
    infoLink.addEventListener("click", e => {

        e.preventDefault();
        GetOrder(order.id);
        // скрываем кнопку создания заказа
        document.getElementById("createOrder").style.display = "none";
    });
    linksTd.append(infoLink);

    const cancelLink = document.createElement("a");
    cancelLink.setAttribute("style", "cursor:pointer;padding:15px;");
    cancelLink.append("Отменить");
    cancelLink.addEventListener("click", e => {
        e.preventDefault();
        CancelOrder(order.id);
    });
    linksTd.append(cancelLink);

    tr.appendChild(linksTd);

    return tr;
}

/// Формирование строки с продуктом для таблицы продуктов в заказе
function CustomerProductRow(product) {

    const tr = document.createElement("tr");
    tr.setAttribute("data-productRowName", product.name);

    const numberTd = document.createElement("td");
    numberTd.append(product.name);
    tr.append(numberTd);

    const priceTd = document.createElement("td");
    priceTd.append(product.price);
    tr.append(priceTd);

    const countTd = document.createElement("td");
    countTd.append(0);
    tr.append(countTd);


    const linksTd = document.createElement("td");

    const addLink = document.createElement("a");
    addLink.setAttribute("style", "cursor:pointer;padding:15px;");
    addLink.append("Добавить");
    addLink.addEventListener("click", e => {
        e.preventDefault();
        customerProducts.push(product.name);
        const productCount = document.querySelector("tr[data-productRowName='" + product.name + "']").cells[2];
        productCount.innerHTML = parseInt(productCount.innerHTML) + 1;
    });
    linksTd.append(addLink);

    const removeLink = document.createElement("a");
    removeLink.setAttribute("style", "cursor:pointer;padding:15px;");
    removeLink.append("Удалить");
    removeLink.addEventListener("click", e => {
        e.preventDefault();
        const index = customerProducts.indexOf(product.name);
        if (index != -1) {
            customerProducts.splice(index, 1);
            const productCount = document.querySelector("tr[data-productRowName='" + product.name + "']").cells[2];
            productCount.innerHTML = parseInt(productCount.innerHTML) - 1;
        }
    });
    linksTd.append(removeLink);

    tr.appendChild(linksTd);

    return tr;
}

/// Обработчик кнопки создания заказа
document.getElementById("createOrder").addEventListener("click", e => {
    e.preventDefault();
    // скрываем список заказов
    document.getElementById("ordersTable").style.display = "none";
    // отображаем форму создания заказа
    const form = document.forms["orderForm"];
    form.style.display = "block";
    form.elements["postamatNumber"].readOnly = false;
    form.reset();
    form.elements["id"].value = 0;

    // очищаем текущий список товаров в заказе
    customerProducts = [];
    // отправляем запрос на получение списка всех товаров
    GetProducts("customerProductsTable", CustomerProductRow);
    // скрываем кнопку создания заказа
    document.getElementById("createOrder").style.display = "none";
});

/// Обрабочик кнопки возвращения к списку заказов
document.getElementById("returnToOrders").addEventListener("click", e => {
    // отображеем список заказов
    document.getElementById("ordersTable").style.display = "block";
    // скрываем форму создания/редактирования заказа
    const form = document.forms["orderForm"];
    form.style.display = "none";
    form.reset();
    form.elements["id"].value = 0;
    // отображаем кнопку создания заказа
    document.getElementById("createOrder").style.display = "block";
});

/// Обработчик кнопки отправки формы заказа
document.forms["orderForm"].addEventListener("submit", e => {
    e.preventDefault();
    const form = document.forms["orderForm"];
    const id = form.elements["id"].value;
    const name = form.elements["customerName"].value;
    const phone = form.elements["phoneNumber"].value;

    if (id == 0) {
        const postamat = form.elements["postamatNumber"].value;
        CreateOrder(name, phone, postamat, customerProducts);
    }
    else
        EditOrder(id, name, phone, customerProducts);
});

/// Отображение вкладки заказов и скрытие остальных
document.getElementById("ordersButton").addEventListener("click", e => {
    e.preventDefault();

    document.forms["orderForm"].style.display = "none";
    document.getElementById("ordersTable").style.display = "block";
    document.getElementById("createOrder").style.display = "block";

    var ordersButtonClasses = document.getElementById("ordersButton").classList;
    ordersButtonClasses.remove("btn-outline-secondary");
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

// #endregion

/// Выход
document.getElementById("exitButton").addEventListener("click", e => {
    e.preventDefault();
    sessionStorage.removeItem(tokenKey);
    window.location.href = "authorization.html";
});