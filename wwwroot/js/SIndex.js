document.querySelectorAll('.dropdown-item').forEach(item => {
	item.addEventListener('click', function (e) {
		e.preventDefault(); var deptId = this.getAttribute('data-deptId');
		var form = this.closest('form');
		var input = document.createElement('input');
		input.type = 'hidden';
		input.name = 'deptId'; input.value = deptId;
		form.appendChild(input);

		form.submit();
	});
});