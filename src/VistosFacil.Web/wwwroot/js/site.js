// Menu mobile
document.querySelectorAll('.nav ul a').forEach(link => {
    link.addEventListener('click', () => {
        document.querySelector('.nav ul')?.classList.remove('open');
    });
});

// Animate cards on scroll
if ('IntersectionObserver' in window) {
    const cards = document.querySelectorAll('.card, .cat-item');
    const obs = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
                obs.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    cards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(16px)';
        card.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
        obs.observe(card);
    });
}
