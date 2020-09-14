$(document).ready(function () {

    var settings = {
        saveState: true,
        onStepChanging: function (event, currentIndex, newIndex) {
            if (currentIndex > newIndex) {
                return true;
            }

            return true;
        },
        onStepChanged: function (event, currentIndex, priorIndex) {
            //if (currentIndex === 2) {
            //    $(this).steps("next");
            //}

            //if (currentIndex === 2 && priorIndex === 3) {
            //    $(this).steps("previous");
            //}
        },
        onFinishing: function (event, currentIndex) {
            return true;
        },
        onFinished: function (event, currentIndex) {
            var form = $(this);

            // Submit form input
            form.submit();
        },
        labels: {
            finish: "Agendar",
            cancel: "Cancelar",
            next: "Avançar",
            previous: "Voltar",
            loading: "Carregando ..."
        }
    };
    $("#wizard").steps(settings);

    var diasDisponiveis = '@Html.Raw(JsonConvert.SerializeObject(new List<string>()));';

    $('.data-disponivel').datepicker({
        format: "dd/mm/yyyy",
        language: "pt-BR",
        beforeShowDay: function (date) {
            var day = String(date.getDate()).padStart(2, '0');
            var month = String(date.getMonth() + 1).padStart(2, '0');
            var year = date.getFullYear();
            var dt_ddmmyyyy = day + '/' + month + '/' + year;
            return (diasDisponiveis.indexOf(dt_ddmmyyyy) != -1);
        },
    }).on('changeDate', function (e) {
        alert(e.format())
    });

});