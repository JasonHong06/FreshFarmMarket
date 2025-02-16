document.addEventListener("DOMContentLoaded", function () {
    const password = document.getElementById('password');
    const confirmPassword = document.getElementById('confirmPassword');
    const profilePhoto = document.getElementById('profilePhoto');
    const togglePassword = document.getElementById('togglePassword');

    // Password Strength Validation
    if (password) {
        password.addEventListener('input', function () {
            var regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@@$!%*?&])[A-Za-z\d@@$!%*?&]{12,}$/;
            document.getElementById('passwordFeedback').style.display = regex.test(password.value) ? "none" : "block";
        });
    }

    // Confirm Password Match
    if (confirmPassword) {
        confirmPassword.addEventListener('input', function () {
            document.getElementById('confirmPasswordFeedback').style.display = (password.value !== confirmPassword.value) ? "block" : "none";
        });
    }

    // Profile Photo Validation (Only JPG/JPEG)
    if (profilePhoto) {
        profilePhoto.addEventListener('change', function () {
            var file = this.files[0];
            var feedback = document.getElementById('profilePhotoFeedback');
            if (file) {
                var validTypes = ['image/jpeg', 'image/jpg'];
                feedback.style.display = validTypes.includes(file.type) ? "none" : "block";
            }
        });
    }

    // Password Visibility Toggle
    if (togglePassword) {
        togglePassword.addEventListener('click', function () {
            password.type = password.type === "password" ? "text" : "password";
        });
    }

    // Auto Logout after Session Timeout
    setInterval(() => {
        fetch('/session-status')
            .then(response => response.json())
            .then(data => {
                if (!data.isAuthenticated) window.location.href = '/login';
            });
    }, 60000);
});
