// Quick-search + client-side pagination for task card grids.
// Works on any container marked .task-card-grid that holds .task-card-col children.
(function () {
    "use strict";

    var PAGE_SIZE = 9;

    function initGrid(grid) {
        var cols = Array.prototype.slice.call(grid.querySelectorAll(".task-card-col"));
        var searchInput = document.querySelector('[data-card-search="' + grid.id + '"]');
        var pager = document.querySelector('[data-card-pager="' + grid.id + '"]');
        var emptyEl = document.querySelector('[data-card-empty="' + grid.id + '"]');
        var currentPage = 1;

        function filtered() {
            var term = (searchInput ? searchInput.value : "").trim().toLowerCase();
            return cols.filter(function (col) {
                if (!term) return true;
                var hay = (col.getAttribute("data-search") || "").toLowerCase();
                return hay.indexOf(term) !== -1;
            });
        }

        function render() {
            var matches = filtered();
            var pageCount = Math.max(1, Math.ceil(matches.length / PAGE_SIZE));
            if (currentPage > pageCount) currentPage = pageCount;

            cols.forEach(function (c) { c.style.display = "none"; });

            var start = (currentPage - 1) * PAGE_SIZE;
            matches.slice(start, start + PAGE_SIZE).forEach(function (c) {
                c.style.display = "";
            });

            if (emptyEl) emptyEl.style.display = matches.length === 0 ? "block" : "none";

            renderPager(pageCount);
        }

        function renderPager(pageCount) {
            if (!pager) return;
            pager.innerHTML = "";
            if (pageCount <= 1) return;

            var ul = document.createElement("ul");
            ul.className = "pagination pagination-rounded mb-0";

            function pageItem(label, page, disabled, active) {
                var li = document.createElement("li");
                li.className = "page-item" + (disabled ? " disabled" : "") + (active ? " active" : "");
                var a = document.createElement("a");
                a.className = "page-link";
                a.href = "javascript:void(0);";
                a.innerHTML = label;
                a.addEventListener("click", function () {
                    if (disabled || active) return;
                    currentPage = page;
                    render();
                });
                li.appendChild(a);
                return li;
            }

            ul.appendChild(pageItem('<i class="mdi mdi-chevron-left"></i>', currentPage - 1, currentPage === 1, false));
            for (var p = 1; p <= pageCount; p++) {
                ul.appendChild(pageItem(String(p), p, false, p === currentPage));
            }
            ul.appendChild(pageItem('<i class="mdi mdi-chevron-right"></i>', currentPage + 1, currentPage === pageCount, false));

            pager.appendChild(ul);
        }

        if (searchInput) {
            searchInput.addEventListener("input", function () {
                currentPage = 1;
                render();
            });
        }

        render();
    }

    document.addEventListener("DOMContentLoaded", function () {
        Array.prototype.slice.call(document.querySelectorAll(".task-card-grid")).forEach(initGrid);
    });
})();
