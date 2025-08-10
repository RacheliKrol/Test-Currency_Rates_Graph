const apiBaseUrl = 'https://localhost:7101/api/currency/rates';
const currencies = ["USD", "GBP", "SEK", "CHF"];
let chartInstance = null;

async function loadData(period) {
    try {
        const response = await fetch(`${apiBaseUrl}/${period}`);
        if (!response.ok) throw new Error('Network response was not ok');
        const data = await response.json();
        renderChart(data);
    } catch (e) {
        alert('שגיאה בטעינת הנתונים: ' + e.message);
    }
}

function renderChart(data) {
    const ctx = document.getElementById('ratesChart').getContext('2d');

    const dates = [...new Set(data.map(r => r.date))].sort();

    const groupedData = {};
    currencies.forEach(c => groupedData[c] = []);
    currencies.forEach(currency => {
        dates.forEach(date => {
            const rate = data.find(r => r.currencyName === currency && r.date === date);
            groupedData[currency].push(rate ? rate.value : null);
        });
    });

    if (chartInstance) {
        chartInstance.destroy();
    }

    chartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: dates,
            datasets: currencies.map((currency, idx) => ({
                label: currency,
                data: groupedData[currency],
                borderColor: getColor(idx),
                fill: false,
                pointRadius: 3,
                borderWidth: 2
            }))
        },
        options: {
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Date'
                    },
                    ticks: {
                        maxTicksLimit: 10
                    }
                },
                y: {
                    title: {
                        display: true,
                        text: 'Rate'
                    }
                }
            }
        }
    });
}

function getColor(index) {
    const colors = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728'];
    return colors[index % colors.length];
}

const buttons = document.querySelectorAll('#buttons-container button');
buttons.forEach(button => {
    button.addEventListener('click', () => {
        buttons.forEach(b => b.classList.remove('active'));
        button.classList.add('active');
        loadData(button.getAttribute('data-period'));
    });
});

window.onload = () => {
    loadData('week');
};
