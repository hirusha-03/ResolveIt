function showToast(message, type = "success") {
    const container = document.getElementById("notification-container");

    const toast = document.createElement("div");
    toast.className = `toast align-items-center text-bg-${type} border-0 mb-2`;
    toast.setAttribute("role", "alert");
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                    data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    container.appendChild(toast);

    const bsToast = new bootstrap.Toast(toast, { delay: 3000 }); // auto hide after 3s
    bsToast.show();

    toast.addEventListener("hidden.bs.toast", () => toast.remove());
}
document.addEventListener("DOMContentLoaded", () => {
    const confirmModal = document.getElementById("confirmModal");

    confirmModal.addEventListener("show.bs.modal", function (event) {
        const button = event.relatedTarget; // button/link that triggered modal
        const actionText = button.getAttribute("data-action") || "Are you sure?";
        const url = button.getAttribute("data-url") || "#";

        document.getElementById("confirmModalBody").innerText = actionText;
        document.getElementById("confirmModalBtn").setAttribute("href", url);
    });
});
