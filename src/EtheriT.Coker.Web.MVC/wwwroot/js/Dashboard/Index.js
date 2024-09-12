var PageReady = function () {
    const $bars = $("#chart-bars")
    var remote = $bars.data("remotes"); //後端寫好的全站瀏覽人次
    var ctx = document.getElementById("chart-bars").getContext("2d");
    co.Picker.Init($("#InputDate"), { timePicker :false});
    $("#InputDate").on("change", function () {
        const selectedDates = $('#InputDate').val(); // 獲取選擇的日期範圍        
        const datesArray = selectedDates.split('~');
        const startDate = new Date(datesArray[0]);
        const endDate = new Date(datesArray[1]);
        co.Remote.GetRemoteCount({ StartDate: startDate, EndDate: endDate }).done(function (result) {
            console.log(result);
        });
    });
    new Chart(ctx, {
        type: "bar",
        data: {
            labels: remote.WebsitesRemotesDate, //X軸日期資料來源
            datasets: [{
                label: "人次",
                tension: 0.4,
                borderWidth: 0, //長條圖的外框粗細
                borderRadius: 4,
                borderSkipped: false,
                backgroundColor: "rgba(255, 255, 255, 1)",
                data: remote.WebsitesRemotesCount, //Y軸數量資料來源
                maxBarThickness: 25 //長條圖粗細
            }, {
                label: "人數",
                tension: 0.4,
                borderWidth: 0,
                borderRadius: 4,
                borderSkipped: false,
                backgroundColor: "#fea11d", //"rgba(255, 0, 0, .8)",
                data: remote.WebsitesRemotesMemCount,
                maxBarThickness: 20
            }],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    labels: {
                        generateLabels: function (chart) {
                            const original = Chart.defaults.plugins.legend.labels.generateLabels;
                            const labels = original.apply(this, [chart]);

                            // Apply different colors for each dataset
                            labels.forEach(function (label, i) {
                                if (i === 0) {
                                    label.fontColor = "rgba(255, 255, 255, 1)"; // 人次的顏色
                                } else if (i === 1) {
                                    label.fontColor = "#ffe2c7"; // 人數的顏色
                                }
                            });
                            return labels;
                        }
                    }
                }
            },
            interaction: {
                intersect: false,
                mode: 'index',
            },
            scales: {
                y: {
                    grid: {
                        drawBorder: false,
                        display: true,
                        drawOnChartArea: true,
                        drawTicks: false,
                        borderDash: [5, 5],
                        color: 'rgba(255, 255, 255, .2)'
                    },
                    ticks: {
                        suggestedMin: 0,
                        suggestedMax: 500,
                        beginAtZero: true,
                        padding: 10,
                        font: {
                            size: 14,
                            weight: 300,
                            family: "Roboto",
                            style: 'normal',
                            lineHeight: 2
                        },
                        color: "#fff"
                    },
                },
                x: {
                    grid: {
                        drawBorder: false,
                        display: true,
                        drawOnChartArea: true,
                        drawTicks: false,
                        borderDash: [5, 5],
                        color: 'rgba(255, 255, 255, .2)'
                    },
                    ticks: {
                        display: true,
                        color: '#f8f9fa',
                        padding: 10,
                        font: {
                            size: 14,
                            weight: 300,
                            family: "Roboto",
                            style: 'normal',
                            lineHeight: 2
                        },
                    }
                },
            },
        },
    });
    /*var ctx2 = document.getElementById("chart-line").getContext("2d");

    new Chart(ctx2, {
        type: "line",
        data: {
            labels: ["Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            datasets: [{
                label: "Mobile apps",
                tension: 0,
                borderWidth: 0,
                pointRadius: 5,
                pointBackgroundColor: "rgba(255, 255, 255, .8)",
                pointBorderColor: "transparent",
                borderColor: "rgba(255, 255, 255, .8)",
                borderColor: "rgba(255, 255, 255, .8)",
                borderWidth: 4,
                backgroundColor: "transparent",
                fill: true,
                data: [50, 40, 300, 320, 500, 350, 200, 230, 500],
                maxBarThickness: 6

            }],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false,
                }
            },
            interaction: {
                intersect: false,
                mode: 'index',
            },
            scales: {
                y: {
                    grid: {
                        drawBorder: false,
                        display: true,
                        drawOnChartArea: true,
                        drawTicks: false,
                        borderDash: [5, 5],
                        color: 'rgba(255, 255, 255, .2)'
                    },
                    ticks: {
                        display: true,
                        color: '#f8f9fa',
                        padding: 10,
                        font: {
                            size: 14,
                            weight: 300,
                            family: "Roboto",
                            style: 'normal',
                            lineHeight: 2
                        },
                    }
                },
                x: {
                    grid: {
                        drawBorder: false,
                        display: false,
                        drawOnChartArea: false,
                        drawTicks: false,
                        borderDash: [5, 5]
                    },
                    ticks: {
                        display: true,
                        color: '#f8f9fa',
                        padding: 10,
                        font: {
                            size: 14,
                            weight: 300,
                            family: "Roboto",
                            style: 'normal',
                            lineHeight: 2
                        },
                    }
                },
            },
        },
    });

    var ctx3 = document.getElementById("chart-line-tasks").getContext("2d");

    new Chart(ctx3, {
        type: "line",
        data: {
            labels: ["Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            datasets: [{
                label: "Mobile apps",
                tension: 0,
                borderWidth: 0,
                pointRadius: 5,
                pointBackgroundColor: "rgba(255, 255, 255, .8)",
                pointBorderColor: "transparent",
                borderColor: "rgba(255, 255, 255, .8)",
                borderWidth: 4,
                backgroundColor: "transparent",
                fill: true,
                data: [50, 40, 300, 220, 500, 250, 400, 230, 500],
                maxBarThickness: 6

            }],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false,
                }
            },
            interaction: {
                intersect: false,
                mode: 'index',
            },
            scales: {
                y: {
                    grid: {
                        drawBorder: false,
                        display: true,
                        drawOnChartArea: true,
                        drawTicks: false,
                        borderDash: [5, 5],
                        color: 'rgba(255, 255, 255, .2)'
                    },
                    ticks: {
                        display: true,
                        padding: 10,
                        color: '#f8f9fa',
                        font: {
                            size: 14,
                            weight: 300,
                            family: "Roboto",
                            style: 'normal',
                            lineHeight: 2
                        },
                    }
                },
                x: {
                    grid: {
                        drawBorder: false,
                        display: false,
                        drawOnChartArea: false,
                        drawTicks: false,
                        borderDash: [5, 5]
                    },
                    ticks: {
                        display: true,
                        color: '#f8f9fa',
                        padding: 10,
                        font: {
                            size: 14,
                            weight: 300,
                            family: "Roboto",
                            style: 'normal',
                            lineHeight: 2
                        },
                    }
                },
            },
        },
    });*/
}
/*----------------------------------未完成----------------------------------
$(document).ready(function () {
    $('#sendData').on('click', function () {
        const selectedDates = $('#InputDate').val(); // 獲取選擇的日期範圍
        console.log('Selected Dates:', selectedDates); // 確認值是否存在

        if (selectedDates) {
            $.ajax({
                url: '/Dashboard/ProcessDateRange',
                method: 'POST', // 使用 POST 請求
                data: { datetimes: selectedDates },
                success: function (response) {
                    console.log('Server Response:', response);
                    console.log("時間"+response.websitesRemotesDate);
                    // 根據需要更新 UI 或執行其他操作
                    //updateChart(response.WebsitesRemotesDate, response.WebsitesRemotesCount, response.WebsitesRemotesMemCount);
                },
                error: function (xhr, status, error) {
                    console.error('AJAX Error:', status, error);
                }
            });
        } else {
            alert('請選擇日期範圍');
        }
    });
    //更新統計表
    function updateChart(dates, counts, memCounts) {
        const chart = Chart.getChart("chart-bars"); // 找到已有的圖表對象
        if (chart) {
            chart.data.labels = dates; // 更新X軸日期資料
            chart.data.datasets[0].data = counts; // 更新Y軸人次資料
            chart.data.datasets[1].data = memCounts; // 更新Y軸人數資料
            chart.update(); // 重新渲染圖表
        } else {
            console.error('Chart instance not found');
        }
    }
});*/