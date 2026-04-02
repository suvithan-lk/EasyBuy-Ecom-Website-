const ProductModal = ({
  product,
  loading,
  onClose,
  onToggleWishlist,
  onAddToCart,
  formatCurrency,
  formatDate,
}) => {
  if (!product && !loading) {
    return null
  }

  return (
    <div className="modal-overlay" role="presentation" onClick={onClose}>
      <div className="product-view-modal" role="dialog" onClick={(event) => event.stopPropagation()}>
        {loading || !product ? (
          <div className="modal-loader">
            <h2>Loading product details...</h2>
          </div>
        ) : (
          <>
            <div className="modal-gallery">
              <div className="modal-main-image">
                <img src={product.imageUrl} alt={product.name} />
              </div>
              <div className="modal-thumb-row">
                {product.gallery.map((imageUrl) => (
                  <div key={imageUrl} className="modal-thumb">
                    <img src={imageUrl} alt={product.name} />
                  </div>
                ))}
              </div>
            </div>

            <div className="modal-product-copy">
              <button type="button" className="modal-close-button" onClick={onClose}>
                Close
              </button>
              <span className="listing-badge">{product.badge}</span>
              <h2>{product.name}</h2>
              <p className="modal-summary">{product.description}</p>

              <div className="modal-pricing">
                <strong>{formatCurrency(product.price)}</strong>
                <span>{formatCurrency(product.compareAtPrice)}</span>
                <small>{product.discountPercent}% off</small>
              </div>

              <div className="modal-product-stats">
                <span>{product.rating.toFixed(1)} stars</span>
                <span>{product.reviewCount} reviews</span>
                <span>{product.stock} units left</span>
                <span>{product.warrantyMonths} month warranty</span>
              </div>

              <div className="modal-spec-grid">
                {product.specs.map((spec) => (
                  <article key={spec.label}>
                    <span>{spec.label}</span>
                    <strong>{spec.value}</strong>
                  </article>
                ))}
              </div>

              <div className="modal-action-row">
                <button type="button" className="secondary-action" onClick={() => onToggleWishlist(product.id)}>
                  {product.isWishlisted ? 'Remove from wishlist' : 'Save item'}
                </button>
                <button type="button" className="primary-action" onClick={() => onAddToCart(product.id)}>
                  Add to cart
                </button>
              </div>

              <div className="modal-reviews">
                <h3>Customer reviews</h3>
                {product.reviews.map((review) => (
                  <article key={`${review.author}-${review.purchasedOn}`} className="review-tile">
                    <div>
                      <strong>{review.headline}</strong>
                      <span>{review.rating}/5</span>
                    </div>
                    <p>{review.comment}</p>
                    <small>
                      {review.author} · {formatDate(review.purchasedOn)}
                    </small>
                  </article>
                ))}
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  )
}

export default ProductModal
