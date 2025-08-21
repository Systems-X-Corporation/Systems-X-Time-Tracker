$('#showpass').on('click', function () {
    const password = document.querySelector('#user-password');
    const type = password.getAttribute('type') === 'password' ? 'text' : 'password';
    password.setAttribute('type', type);
});


$('#showpass').on('hover', function () {
    alert("eye");
});