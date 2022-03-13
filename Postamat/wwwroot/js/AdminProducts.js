var tokenKey = "accessToken";

/// Получение всех товаров
async function GetProducts() {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/product/all", {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const products = await response.json();
        let rows = document.getElementById("productsTable");
        rows.innerHTML = "";
        products.forEach(product => {
            rows.append(ProductRow(product));
        });
    }
}

/// Получение товара по id
async function GetProduct(id) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/product/" + id, {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const product = await response.json();
        const form = document.forms["productForm"];
        form.elements["id"].value = product.id;
        form.elements["name"].value = product.name;
        form.elements["price"].value = product.price;
    }
}

/// Создание товара
async function CreateProduct(productName, productPrice) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("api/product", {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token
        },
        body: JSON.stringify({
            name: productName,
            price: productPrice
        })
    });
    const product = await response.json();
    if (response.ok === true) {
        ResetProductForm();
        document.getElementById("productsTable").append(ProductRow(product));
    }
    else {
        alert("Error: " + response.status + "; " + product.errorText);
    }
}

/// Редактирование товара
async function EditProduct(productId, productName, productPrice) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("api/product", {
        method: "PUT",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token
        },
        body: JSON.stringify({
            id: parseInt(productId, 10),
            name: productName,
            price: productPrice
        })
    });
    const product = await response.json();
    if (response.ok === true) {
        ResetProductForm();
        document.querySelector("tr[data-productRowId='" + product.id + "']").replaceWith(ProductRow(product));
    }
    else {
        alert("Error: " + response.status + "; " + product.errorText);
    }
}

/// Удаление товара
async function DeleteProduct(id) {
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/api/product/" + id, {
        method: "DELETE",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });
    if (response.ok === true) {
        const product = await response.json();
        document.querySelector("tr[data-productRowId='" + product.id + "']").remove();
        ResetProductForm();
    }
}

/// Сброс формы товара
function ResetProductForm() {
    const form = document.forms["productForm"];
    form.reset();
    form.elements["id"].value = 0;
}

/// Формирование строки с товаром
function ProductRow(product) {

    const tr = document.createElement("tr");
    tr.setAttribute("data-productRowId", product.id);

    const idTd = document.createElement("td");
    idTd.append(product.id);
    tr.append(idTd);

    const nameTd = document.createElement("td");
    nameTd.append(product.name);
    tr.append(nameTd);

    const priceTd = document.createElement("td");
    priceTd.append(product.price);
    tr.append(priceTd);


    const linksTd = document.createElement("td");

    const editLink = document.createElement("a");
    editLink.setAttribute("style", "cursor:pointer;padding:15px;");
    editLink.append("Изменить");
    editLink.addEventListener("click", e => {

        e.preventDefault();
        GetProduct(product.id);
    });
    linksTd.append(editLink);

    const removeLink = document.createElement("a");
    removeLink.setAttribute("style", "cursor:pointer;padding:15px;");
    removeLink.append("Удалить");
    removeLink.addEventListener("click", e => {

        e.preventDefault();
        DeleteProduct(product.id);
    });

    linksTd.append(removeLink);
    tr.appendChild(linksTd);

    return tr;
}

/// Обработчик кнопки отправки формы товара
document.forms["productForm"].addEventListener("submit", e => {
    e.preventDefault();
    const form = document.forms["productForm"];
    const id = form.elements["id"].value;
    const name = form.elements["name"].value;
    const price = form.elements["price"].value;
    if (id == 0)
        CreateProduct(name, price);
    else
        EditProduct(id, name, price);
});

/// Обработчик кнопки сброса формы товара
document.getElementById("productFormReset").addEventListener("click", e => {
    e.preventDefault();
    ResetProductForm();
})

/// Отображение вкладки товаров и скрытие остальных
document.getElementById("productsButton").addEventListener("click", e => {
    e.preventDefault();

    var productsButtonClasses = document.getElementById("productsButton").classList;
    productsButtonClasses.remove("btn-outline-secondary")
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

    GetProducts();
})