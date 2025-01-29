function showAlert(title, message) {
    let content = message.indexOf("<") ? message : `<p>${message}<p>`;
	let element = document.createElement("div");
	element.classList = ["modal fade"];
	element.tabIndex = "-1";
	element.id = uuidv4();
    element.innerHTML =
    `<div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h1 class="modal-title fs-5" id="exampleModalLabel">${title}</h1>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            ${content}
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
          </div>
        </div>
    </div>`;

    let body = document.getElementsByTagName("body")[0];
    body.innerHTML += element.outerHTML;
    const modal = new bootstrap.Modal(element, {});
    modal.show();

    element.addEventListener('hidden.bs.modal', function () {
        modal.hide();
        modal.dispose();
        element.remove();
    });
}

function uuidv4() {
	return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, c =>
		(+c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> +c / 4).toString(16)
	);
}