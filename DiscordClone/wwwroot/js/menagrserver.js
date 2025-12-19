document.addEventListener("DOMContentLoaded", () => {
    const buttons = document.querySelectorAll(".sidebar-btn");
    const sections = document.querySelectorAll(".settings-section");

    // ---- ZMIANA SEKCJI ----
    buttons.forEach(button => {
        button.addEventListener("click", () => {

            // active button
            buttons.forEach(b => b.classList.remove("active"));
            button.classList.add("active");

            const sectionName = button.dataset.section;

            // show proper section
            sections.forEach(section => {
                section.classList.remove("active");

                if (section.id === `section-${sectionName}`) {
                    section.classList.add("active");
                }
            });
        });
    });

    // ---- PODGLĄD AVATARA ----
    const avatarInput = document.getElementById("avatarInput");
    const avatarPreview = document.getElementById("avatarPreview");

    if (avatarInput && avatarPreview) {
        avatarInput.addEventListener("change", () => {
            const file = avatarInput.files[0];
            if (!file) return;

            const reader = new FileReader();
            reader.onload = e => {
                avatarPreview.src = e.target.result;
            };
            reader.readAsDataURL(file);
        });
    }
});
