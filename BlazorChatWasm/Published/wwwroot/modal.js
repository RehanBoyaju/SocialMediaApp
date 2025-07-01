window.showModal = (id) => {
    let modalElement = document.getElementById(id);
    if (!modalElement) {
        console.error(`Modal with ID '${id}' not found.`);
        return;
    }

    let myModal = new bootstrap.Modal(modalElement);
    myModal.show();
};

window.hideModal = (id) => {
    let modalElement = document.getElementById(id);
    let modalInstance = bootstrap.Modal.getInstance(modalElement);
    if (modalInstance) {
        modalInstance.hide();
    }
};


