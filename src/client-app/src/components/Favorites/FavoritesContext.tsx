import React, { createContext, useCallback, useContext, useEffect, useState } from "react";
import { addToFavorites as addToFavoritesApi, getList, removeFromFavorites as removeFromFavoritesApi } from "src/shared/CookTime";
import { useAuthentication } from "../Authentication/AuthenticationContext";

interface FavoritesContextType {
    favoriteIds: Set<string>;
    isFavorite: (recipeId: string) => boolean;
    addToFavorites: (recipeId: string) => Promise<void>;
    removeFromFavorites: (recipeId: string) => Promise<void>;
    toggleFavorite: (recipeId: string) => Promise<void>;
    loading: boolean;
}

const FavoritesContext = createContext<FavoritesContextType>({
    favoriteIds: new Set(),
    isFavorite: () => false,
    addToFavorites: async () => { },
    removeFromFavorites: async () => { },
    toggleFavorite: async () => { },
    loading: true,
});

export function useFavorites() {
    return useContext(FavoritesContext);
}

export function FavoritesProvider({ children }: { children: React.ReactNode }) {
    const [favoriteIds, setFavoriteIds] = useState<Set<string>>(new Set());
    const [loading, setLoading] = useState(true);
    const { user } = useAuthentication();

    useEffect(() => {
        async function loadFavorites() {
            if (!user) {
                setFavoriteIds(new Set());
                setLoading(false);
                return;
            }

            try {
                const list = await getList("Favorites");
                if (list?.recipes) {
                    setFavoriteIds(new Set(list.recipes.map(r => r.id)));
                } else {
                    setFavoriteIds(new Set());
                }
            } catch {
                setFavoriteIds(new Set());
            }
            setLoading(false);
        }

        setLoading(true);
        loadFavorites();
    }, [user]);

    const isFavorite = useCallback((recipeId: string) => {
        return favoriteIds.has(recipeId);
    }, [favoriteIds]);

    const addToFavorites = useCallback(async (recipeId: string) => {
        await addToFavoritesApi(recipeId);
        setFavoriteIds(prev => new Set([...prev, recipeId]));
    }, []);

    const removeFromFavorites = useCallback(async (recipeId: string) => {
        await removeFromFavoritesApi(recipeId);
        setFavoriteIds(prev => {
            const next = new Set(prev);
            next.delete(recipeId);
            return next;
        });
    }, []);

    const toggleFavorite = useCallback(async (recipeId: string) => {
        if (favoriteIds.has(recipeId)) {
            await removeFromFavorites(recipeId);
        } else {
            await addToFavorites(recipeId);
        }
    }, [favoriteIds, addToFavorites, removeFromFavorites]);

    return (
        <FavoritesContext.Provider value={{
            favoriteIds,
            isFavorite,
            addToFavorites,
            removeFromFavorites,
            toggleFavorite,
            loading,
        }}>
            {children}
        </FavoritesContext.Provider>
    );
}
