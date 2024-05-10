// Dati di esempio per il grafico della linea
var lineChartData = {
    labels: ["Gen", "Feb", "Mar", "Apr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dic"],
    datasets: [{
        label: 'My Dataset',
        data: [65, 59, 80, 81, 56, 55, 40, 50, 72, 65, 82, 75],
        fill: false,
        borderColor: 'rgb(75, 192, 192)',
        tension: 0.1
    }]
};

// Opzioni per il grafico della linea
var lineChartOptions = {
    responsive: true
};

// Ottieni il contesto del canvas
var ctx = document.getElementById('lineChart').getContext('2d');

// Crea il grafico della linea
new Chart(ctx, {
    type: 'line',
    data: lineChartData,
    options: lineChartOptions
});

// Funzione per il calcolo totale degli utenti registrati
document.addEventListener('DOMContentLoaded', function () {
    fetch('/Utenti/GetTotalUsers')
        .then(response => {
            if (!response.ok) {
                throw new Error('Errore nel caricamento dei dati');
            }
            return response.json();
        })
        .then(data => {
            document.getElementById('totalUsers').textContent = data;
        })
        .catch(error => {
            console.error(error);
            document.getElementById('totalUsers').textContent = 'Errore nel caricamento dei dati';
        });
});

//Funzione per il numero totale di collezioni listate
document.addEventListener('DOMContentLoaded', function () {
    fetch('/Collezioni/GetTotalCollections')
        .then(response => {
            if (!response.ok) {
                throw new Error('Errore nel caricamento dei dati');
            }
            return response.json();
        })
        .then(data => {
            document.getElementById('totalCollections').textContent = data;
        })
        .catch(error => {
            console.error(error);
            document.getElementById('totalCollections').textContent = 'Errore nel caricamento dei dati';
        });
});

// Funzione per il saldo del sito
    document.addEventListener('DOMContentLoaded', function () {
        fetch('/Wallets/GetAdminWalletBalance')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Errore nel caricamento dei dati');
                }
                return response.json();
            })
            .then(data => {
                var saldoEPC = data + " EPC";
                document.getElementById('adminWalletBalance').textContent = saldoEPC;
            })
            .catch(error => {
                console.error(error);
                document.getElementById('adminWalletBalance').textContent = 'Errore nel caricamento dei dati';
            });
    });

    // Funzione per il calcolo del numero di transazioni giornaliere
    document.addEventListener('DOMContentLoaded', function () {
        // Effettua una chiamata AJAX per recuperare il numero di transazioni del giorno
        fetch('/Transazioni/GetDailyTransactionsCount')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Errore nel caricamento dei dati');
                }
                return response.json();
            })
            .then(data => {
                // Aggiorna il contenuto del placeholder con il numero di transazioni del giorno
                document.getElementById('dailyTransactions').textContent = data;
            })
            .catch(error => {
                console.error(error);
                document.getElementById('dailyTransactions').textContent = 'Errore nel caricamento dei dati';
            });
    });