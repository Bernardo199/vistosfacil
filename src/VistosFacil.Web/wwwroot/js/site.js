function toggleMenu() {
  document.getElementById('main-nav').classList.toggle('open');
}
document.addEventListener('click', function(e) {
  const nav = document.getElementById('main-nav');
  const hamburger = document.getElementById('hamburger');
  if (nav && hamburger && !nav.contains(e.target) && !hamburger.contains(e.target)) {
    nav.classList.remove('open');
  }
});
if ('IntersectionObserver' in window) {
  const obs = new IntersectionObserver((entries) => {
    entries.forEach((entry, i) => {
      if (entry.isIntersecting) {
        setTimeout(() => { entry.target.style.opacity='1'; entry.target.style.transform='translateY(0)'; }, i*80);
        obs.unobserve(entry.target);
      }
    });
  }, { threshold: 0.1 });
  document.querySelectorAll('.card,.article-card,.dest-card').forEach(el => {
    el.style.opacity='0'; el.style.transform='translateY(16px)'; el.style.transition='opacity 0.4s ease,transform 0.4s ease';
    obs.observe(el);
  });
}
window.addEventListener('scroll', function() {
  const h = document.querySelector('header');
  if (h) h.style.boxShadow = window.scrollY > 10 ? '0 4px 20px rgba(0,0,0,.12)' : '0 2px 8px rgba(0,0,0,.06)';
});
