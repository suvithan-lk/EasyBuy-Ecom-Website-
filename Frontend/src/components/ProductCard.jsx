const ProductCard = ({
  product,
  onView,
  onAddToCart,
  onToggleWishlist,
  formatCurrency,
  busyProductId,
}) => {
  const isBusy = busyProductId === product.id

  return (
    <article className="listing-card">
      <button
        type="button"
        className={`wishlist-toggle ${product.isWishlisted ? 'active' : ''}`}
        onClick={() => onToggleWishlist(product.id)}
      >
        {product.isWishlisted ? 'Saved' : 'Save'}
      </button>

      <button type="button" className="listing-image-shell" onClick={() => onView(product.id)}>
        <img src={product.imageUrl} alt={product.name} loading="lazy" />
      </button>

      <div className="listing-copy">
        <span className="listing-badge">{product.badge}</span>
        <button type="button" className="listing-title" onClick={() => onView(product.id)}>
          {product.name}
        </button>
        <p>{product.summary}</p>

        <div className="listing-rating">
          <strong>{product.rating.toFixed(1)}</strong>
          <span>{product.reviewCount} reviews</span>
        </div>

        <div className="listing-price">
          <strong>{formatCurrency(product.price)}</strong>
          <span>{formatCurrency(product.compareAtPrice)}</span>
        </div>

        <div className="listing-meta">
          <span>{product.shippingDays}-day shipping</span>
          <span>{product.stock > 0 ? `${product.stock} in stock` : 'Out of stock'}</span>
        </div>

        <div className="listing-tags">
          {product.tags.slice(0, 3).map((tag) => (
            <span key={tag}>{tag}</span>
          ))}
        </div>

        <button
          type="button"
          className="listing-cart-button"
          disabled={product.stock === 0 || isBusy}
          onClick={() => onAddToCart(product.id)}
        >
          {isBusy ? 'Updating...' : 'Add to Cart'}
        </button>
      </div>
    </article>
  )
}

export default ProductCard
