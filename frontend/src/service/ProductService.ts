export interface Product {
    id?: string;
    code?: string;
    name?: string;
    description?: string;
    image?: string;
    price?: number;
    category?: string;
    quantity?: number;
    inventoryStatus?: string;
    rating?: number;
    orders?: any[];
}

export const ProductService = {
    async getProductsSmall(): Promise<Product[]> {
        try {
            const response = await fetch('/demo/data/products-small.json');
            const data = await response.json();
            return data.data || [];
        } catch (error) {
            console.error('Error loading products small:', error);
            return [];
        }
    },

    async getProducts(): Promise<Product[]> {
        try {
            const response = await fetch('/demo/data/products.json');
            const data = await response.json();
            return data.data || [];
        } catch (error) {
            console.error('Error loading products:', error);
            return [];
        }
    },

    async getProductsWithOrdersSmall(): Promise<Product[]> {
        try {
            const response = await fetch('/demo/data/products-orders-small.json');
            const data = await response.json();
            return data.data || [];
        } catch (error) {
            console.error('Error loading products with orders:', error);
            return [];
        }
    }
};
